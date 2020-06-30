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

namespace Basique.Solve
{
    public static class QuerySolver
    {
        public static async ValueTask<object> SolvePullQuery(List<ExpressionNode> expr, CancellationToken token, IRelation table)
        {
            SqlSelectorData data = SqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            List<object> res = new List<object>();
            if (!(expr[^1] is PullExpressionNode))
            {
                DbCommand command = table.Schema.Connection.CreateCommand();
                command.Transaction = data.Transaction?.wrapping;
                SqlBuilder.WriteSqlSelect(data, command);
                table.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                return typeof(BasiqueEnumerator<>).MakeGenericType(table.ElementType).GetConstructor(new[] { typeof(DbDataReader), typeof(CancellationToken), typeof(IRelation) }).Invoke(new object[] { await command.ExecuteReaderAsync(token), token, table });
            }
            await using (DbCommand command = table.Schema.Connection.CreateCommand())
            {
                command.Transaction = data.Transaction?.wrapping;
                SqlBuilder.WriteSqlSelect(data, command);
                table.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
                await using (var reader = await command.ExecuteReaderAsync(token))
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync(token))
                        {
                            object obj = Activator.CreateInstance(data.RequestedType);
                            foreach (var columnData in table.Schema.Tables[data.RequestedType].Columns.Values)
                            {
                                object val = Convert.ChangeType(reader.GetValue(columnData.Name), columnData.Type);
                                columnData.Path.Set(obj, val);
                            }
                            res.Add(obj);
                        }
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
            SqlUpdateData data = SqlBuilder.BuildUpdateData(expr);
            await using DbCommand command = tab.Schema.Connection.CreateCommand();
            command.Transaction = data.Transaction?.wrapping;
            SqlBuilder.WriteSqlUpdate(data, command);
            tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
            await command.ExecuteNonQueryAsync(token);
            return null;
        }

        public static async ValueTask<object> SolvePullSingleQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab)
        {
            SqlSelectorData data = SqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            PullSingleExpressionNode node = expr[^1] as PullSingleExpressionNode;
            await using (DbCommand command = tab.Schema.Connection.CreateCommand())
            {
                command.Transaction = data.Transaction?.wrapping;
                SqlBuilder.WriteSqlPullSingle(data, node, command);
                tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");

                await using (var reader = await command.ExecuteReaderAsync(token))
                {
                    if (!reader.HasRows)
                    {
                        if (node.IncludeDefault)
                            return null;
                        else
                            throw new InvalidOperationException("The source sequence is empty.");
                    }
                    object res = Activator.CreateInstance(data.RequestedType);
                    await reader.ReadAsync(token);
                    foreach (var pair in tab.Schema.Tables[data.RequestedType].Columns.Values)
                    {
                        object val = reader.GetValue(pair.Name);
                        object valConv = Convert.ChangeType(val, pair.Type);
                        pair.Path.Set(res, valConv);
                    }
                    if (await reader.ReadAsync(token) && node.Type == PullSingleExpressionNode.PullType.Single)
                        throw new InvalidOperationException("More than one element satisfies the condition in predicate.");
                    return res;
                }
            }
        }

        public static async ValueTask<object> SolveDeleteQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab)
        {
            SqlSelectorData data = SqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            await using DbCommand command = tab.Schema.Connection.CreateCommand();
            command.Transaction = data.Transaction?.wrapping;
            SqlBuilder.WriteSqlDelete(data, tab, command);
            tab.Schema.Logger.Log(LogLevel.Debug, $"Running SQL: {command.CommandText}");
            await command.ExecuteNonQueryAsync(token);
            return null;
        }

        public static async ValueTask<object> SolveCreateQuery(List<ExpressionNode> pn, CancellationToken token, IRelation tab)
        {
            CreateExpressionNode create = pn.Last() as CreateExpressionNode;
            await using (DbCommand command = tab.Schema.Connection.CreateCommand())
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