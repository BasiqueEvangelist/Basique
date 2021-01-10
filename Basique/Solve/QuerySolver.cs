using System.Net.Mail;
using System.Threading;
using System.Linq.Expressions;
using Basique.Modeling;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System;
using System.Reflection;
using System.Data;
using System.Linq;
using Basique.Services;
using Basique.Flattening;

namespace Basique.Solve
{
    public static class QuerySolver
    {
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
                return Activator.CreateInstance(typeof(BasiqueEnumerator<>).MakeGenericType(table.ElementType), new object[] { await command.ExecuteReaderAsync(token), token, conn, data.Columns, trans == null });
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
                        object obj = Activator.CreateInstance(data.RequestedType);
                        foreach (var (path, column) in data.Columns.WalkColumns())
                        {
                            object val = Convert.ChangeType(reader.GetValue(column.NamedAs), column.Column.Type);
                            path.Set(obj, val);
                        }
                        res.Add(obj);
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
                object res = Activator.CreateInstance(data.RequestedType);
                await reader.ReadAsync(token);
                foreach (var (path, column) in data.Columns.WalkColumns())
                {
                    object val = Convert.ChangeType(reader.GetValue(column.NamedAs), column.Column.Type);
                    path.Set(res, val);
                }
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
            CreateExpressionNode create = pn.Last() as CreateExpressionNode;
            DbTransaction trans = pn.OfType<TransactionExpressionNode>().SingleOrDefault()?.Transaction?.wrapping;
            DbConnection conn = trans == null ? await tab.Schema.MintConnection() : trans.Connection;
            await using (new DisposePredicate(conn, trans == null))
            await using (DbCommand command = conn.CreateCommand())
            {
                command.Transaction = pn.OfType<TransactionExpressionNode>().SingleOrDefault()?.Transaction?.wrapping;
                SqlBuilder.WriteSqlCreate(create, tab, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                await command.ExecuteNonQueryAsync(token);
            }
            object inst = Activator.CreateInstance(create.OfType);
            foreach (var binding in create.Factory.Bindings)
            {
                if (binding is MemberAssignment assign)
                {
                    object value = Expression.Lambda(assign.Expression).Compile(true).DynamicInvoke();
                    if (assign.Member is FieldInfo field)
                        field.SetValue(inst, value);
                    else if (assign.Member is PropertyInfo property)
                        property.SetValue(inst, value);
                    else
                        throw new NotImplementedException();
                }
                else
                    throw new NotImplementedException();
            }
            return inst;
        }
    }
}