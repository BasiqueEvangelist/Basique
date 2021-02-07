using System.Threading;
using Basique.Modeling;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Linq;
using Basique.Services;
using Basique.Flattening;

namespace Basique.Solve
{
    public static class QuerySolver
    {
        public static PathTreeElement<T> CreateTreeElement<T>(bool isTree)
        {
            return isTree ? new PathTreeElement<T>(new PathTree<T>()) : new PathTreeElement<T>(default(T));
        }

        public static async ValueTask<object> SolvePullQuery(List<ExpressionNode> expr, CancellationToken token, IRelation table)
        {
            SqlSelectorData data = LinqVM.BuildSelectorData(expr, new SqlSelectorData());
            List<object> res = new();
            DbTransaction trans = data.Transaction?.wrapping;
            DbConnection conn = trans == null ? await table.Schema.MintConnection() : trans.Connection;
            if (!(expr[^1] is PullExpressionNode))
            {
                DbCommand command = conn.CreateCommand();
                command.Transaction = trans;
                SqlBuilder.WriteSqlSelect(data, command);
                table.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                return Activator.CreateInstance(typeof(BasiqueEnumerator<>).MakeGenericType(table.ElementType), new object[] { table.Schema, await command.ExecuteReaderAsync(token), token, conn, data.Columns, trans == null });
            }
            await using (new DisposePredicate(conn, trans == null))
            await using (DbCommand command = conn.CreateCommand())
            {
                command.Transaction = trans;
                SqlBuilder.WriteSqlSelect(data, command);
                table.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                await using var reader = await command.ExecuteReaderAsync(token);
                if (reader.HasRows)
                    while (await reader.ReadAsync(token))
                    {
                        var newSet = CreateTreeElement<object>(data.Columns.IsTree);
                        foreach (var (path, column) in data.Columns.WalkValues())
                        {
                            object orig = reader.GetValue(column.NamedAs);
                            if (!table.Schema.Converter.TryConvert(orig, column.Column.Type, out var val)) throw new InvalidOperationException($"Could not translate {orig.GetType()} to {column.Column.Type}");
                            newSet.Set(path, val);
                        }
                        res.Add(ObjectFactory.Create(data.RequestedType, newSet));
                    }
            }
            if ((expr.Last() as PullExpressionNode).Type == PullExpressionNode.PullType.Array)
                return GenericUtils.MakeGenericArray(res, data.RequestedType);
            else if ((expr.Last() as PullExpressionNode).Type == PullExpressionNode.PullType.List)
                return GenericUtils.MakeGenericList(res, data.RequestedType);
            else
                throw new NotImplementedException();
        }

        public static async ValueTask<object> SolveUpdateQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab)
        {
            SqlUpdateData data = LinqVM.BuildUpdateData(expr);
            DbTransaction trans = data.Transaction?.wrapping;
            DbConnection conn = trans == null ? await tab.Schema.MintConnection() : trans.Connection;
            await using (new DisposePredicate(conn, trans == null))
            {
                await using DbCommand command = conn.CreateCommand();
                command.Transaction = trans;
                SqlBuilder.WriteSqlUpdate(data, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                await command.ExecuteNonQueryAsync(token);
            }
            return null;
        }

        public static async ValueTask<object> SolvePullSingleQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab)
        {
            SqlSelectorData data = LinqVM.BuildSelectorData(expr, new SqlSelectorData());
            PullSingleExpressionNode node = expr[^1] as PullSingleExpressionNode;
            DbTransaction trans = data.Transaction?.wrapping;
            DbConnection conn = trans == null ? await tab.Schema.MintConnection() : trans.Connection;
            await using (new DisposePredicate(conn, trans == null))
            await using (DbCommand command = conn.CreateCommand())
            {
                command.Transaction = trans;
                SqlBuilder.WriteSqlPullSingle(data, node, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");

                await using var reader = await command.ExecuteReaderAsync(token);
                if (!reader.HasRows)
                {
                    if (node.IncludeDefault)
                        return null;
                    else
                        throw new InvalidOperationException("The source sequence is empty.");
                }
                var newSet = CreateTreeElement<object>(data.Columns.IsTree);
                await reader.ReadAsync(token);
                foreach (var (path, column) in data.Columns.WalkValues())
                {
                    object orig = reader.GetValue(column.NamedAs);
                    if (!tab.Schema.Converter.TryConvert(orig, column.Column.Type, out var val)) throw new InvalidOperationException($"Could not translate {orig.GetType()} to {column.Column.Type}");
                    newSet.Set(path, val);
                }
                var res = ObjectFactory.Create(data.RequestedType, newSet);

                if (await reader.ReadAsync(token) && node.Type == PullSingleExpressionNode.PullType.Single)
                    throw new InvalidOperationException("More than one element satisfies the condition in predicate.");
                return res;
            }
        }

        public static async ValueTask<object> SolveDeleteQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab)
        {
            SqlSelectorData data = LinqVM.BuildSelectorData(expr, new SqlSelectorData());
            DbTransaction trans = data.Transaction?.wrapping;
            DbConnection conn = trans == null ? await tab.Schema.MintConnection() : trans.Connection;
            await using (new DisposePredicate(conn, trans == null))
            {
                await using DbCommand command = conn.CreateCommand();
                command.Transaction = trans;
                SqlBuilder.WriteSqlDelete(data, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                await command.ExecuteNonQueryAsync(token);
            }
            return null;
        }

        public static async ValueTask<object> SolveCreateQuery(List<ExpressionNode> pn, CancellationToken token, IRelation tab)
        {
            CreateExpressionNode create = pn.OfType<CreateExpressionNode>().Single();
            DbTransaction trans = pn.OfType<TransactionExpressionNode>().SingleOrDefault()?.Transaction?.wrapping;
            DbConnection conn = trans == null ? await tab.Schema.MintConnection() : trans.Connection;
            await using (new DisposePredicate(conn, trans == null))
            {
                var startSet = LinqVM.BuildSimpleColumnSet(tab);
                await using DbCommand command = conn.CreateCommand();
                command.Transaction = pn.OfType<TransactionExpressionNode>().SingleOrDefault()?.Transaction?.wrapping;
                SqlBuilder.WriteSqlCreate(startSet, create, tab, command);
                if (pn[^1] is VoidExpressionNode)
                {
                    tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                    await command.ExecuteNonQueryAsync(token);
                    return null;
                }
                var data = LinqVM.BuildCreateData(pn, new());
                SqlBuilder.WriteSqlPullCreated(startSet, data, tab, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                await using var reader = await command.ExecuteReaderAsync(token);
                var newSet = CreateTreeElement<object>(data.Columns.IsTree);
                await reader.ReadAsync(token);
                foreach (var (path, column) in data.Columns.WalkValues())
                {
                    object orig = reader.GetValue(column.NamedAs);
                    if (!tab.Schema.Converter.TryConvert(orig, column.Column.Type, out var val)) throw new InvalidOperationException($"Could not translate {orig.GetType()} to {column.Column.Type}");
                    newSet.Set(path, val);
                }
                return ObjectFactory.Create(data.RequestedType, newSet);
            }
        }

        public static async ValueTask<object> SolveCountQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab)
        {
            SqlSelectorData data = LinqVM.BuildSelectorData(expr, new SqlSelectorData());
            CountExpressionNode node = expr[^1] as CountExpressionNode;
            DbTransaction trans = data.Transaction?.wrapping;
            DbConnection conn = trans == null ? await tab.Schema.MintConnection() : trans.Connection;
            await using (new DisposePredicate(conn, trans == null))
            await using (DbCommand command = conn.CreateCommand())
            {
                command.Transaction = trans;
                SqlBuilder.WriteSqlCount(data, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");

                var count = (long)await command.ExecuteScalarAsync(token);

                if (node.Type == CountExpressionNode.CountType.Count)
                    return (int)count;
                else if (node.Type == CountExpressionNode.CountType.LongCount)
                    return count;
                else if (node.Type == CountExpressionNode.CountType.Any)
                    return count != 0;
                else
                    return count == 0;
            }
        }
    }
}