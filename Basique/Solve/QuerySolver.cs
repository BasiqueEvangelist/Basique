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
using NLog;

namespace Basique.Solve
{
    public static class QuerySolver
    {
        private static Logger LOGGER = LogManager.GetCurrentClassLogger();

        public static async ValueTask<object> SolvePullQuery(List<ExpressionNode> expr, CancellationToken token, ITable table)
        {
            SqlSelectorData data = SqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            List<object> res = new List<object>();
            await using (DbCommand command = table.Context.Connection.CreateCommand())
            {
                SqlBuilder.WriteSqlSelect(data, command);
                LOGGER.Debug("Running SQL: {0}", command.CommandText);
                await using (var reader = await command.ExecuteReaderAsync(token))
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync(token))
                        {
                            object obj = Activator.CreateInstance(data.RequestedType);
                            foreach (var pair in table.Context.Tables[data.RequestedType].Columns)
                            {
                                object val = Convert.ChangeType(reader.GetValue(pair.Value.Name), pair.Value.Of);
                                if (pair.Key is FieldInfo field)
                                    field.SetValue(obj, val);
                                else if (pair.Key is PropertyInfo prop)
                                    prop.SetValue(obj, val);
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

        public static async ValueTask<object> SolveUpdateQuery(List<ExpressionNode> expr, CancellationToken token, ITable tab)
        {
            SqlUpdateData data = SqlBuilder.BuildUpdateData(expr);
            await using DbCommand command = tab.Context.Connection.CreateCommand();
            SqlBuilder.WriteSqlUpdate(data, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            await command.ExecuteNonQueryAsync(token);
            return null;
        }

        public static async ValueTask<object> SolvePullSingleQuery<T>(List<ExpressionNode> expr, CancellationToken token, Table<T> tab)
        {
            SqlSelectorData data = SqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            PullSingleExpressionNode node = expr[^1] as PullSingleExpressionNode;
            await using (DbCommand command = tab.Context.Connection.CreateCommand())
            {
                SqlBuilder.WriteSqlPullSingle(data, node, command);
                LOGGER.Debug("Running SQL: {0}", command.CommandText);

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
                    foreach (var pair in tab.Context.Tables[data.RequestedType].Columns)
                    {
                        object val = Convert.ChangeType(reader.GetValue(pair.Value.Name), pair.Value.Of);
                        if (pair.Key is FieldInfo field)
                            field.SetValue(res, val);
                        else if (pair.Key is PropertyInfo prop)
                            prop.SetValue(res, val);
                    }
                    if (await reader.ReadAsync(token) && node.Type == PullSingleExpressionNode.PullType.Single)
                        throw new InvalidOperationException("More than one element satisfies the condition in predicate.");
                    return res;
                }
            }
        }

        public static async ValueTask<object> SolveDeleteQuery<T>(List<ExpressionNode> expr, CancellationToken token, Table<T> tab)
        {
            SqlSelectorData data = SqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            await using DbCommand command = tab.Context.Connection.CreateCommand();
            SqlBuilder.WriteSqlDelete(data, tab, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            await command.ExecuteNonQueryAsync(token);
            return null;
        }

        public static async ValueTask<object> SolveCreateQuery(List<ExpressionNode> pn, CancellationToken token, ITable tab)
        {
            CreateExpressionNode create = pn.Last() as CreateExpressionNode;
            await using (DbCommand command = tab.Context.Connection.CreateCommand())
            {
                SqlBuilder.WriteSqlCreate(create, tab, command);
                LOGGER.Debug("Running SQL: {0}", command.CommandText);
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