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
using Perfusion;

namespace Basique.Solve
{
    public class QuerySolver
    {
        [Inject] private ILogger LOGGER;
        [Inject] private SqlBuilder sqlBuilder;

        public async ValueTask<object> SolvePullQuery(List<ExpressionNode> expr, CancellationToken token, IRelation table, DbTransaction transaction)
        {
            SqlSelectorData data = sqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            List<object> res = new List<object>();
            await using (DbCommand command = table.Context.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                sqlBuilder.WriteSqlSelect(data, command);
                LOGGER.Debug("Running SQL: {0}", command.CommandText);
                await using (var reader = await command.ExecuteReaderAsync(token))
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync(token))
                        {
                            object obj = Activator.CreateInstance(data.RequestedType);
                            foreach (var columnData in table.Context.Tables[data.RequestedType].Columns.Values)
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

        public async ValueTask<object> SolveUpdateQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab, DbTransaction transaction)
        {
            SqlUpdateData data = sqlBuilder.BuildUpdateData(expr);
            await using DbCommand command = tab.Context.Connection.CreateCommand();
            command.Transaction = transaction;
            sqlBuilder.WriteSqlUpdate(data, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            await command.ExecuteNonQueryAsync(token);
            return null;
        }

        public async ValueTask<object> SolvePullSingleQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab, DbTransaction transaction)
        {
            SqlSelectorData data = sqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            PullSingleExpressionNode node = expr[^1] as PullSingleExpressionNode;
            await using (DbCommand command = tab.Context.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                sqlBuilder.WriteSqlPullSingle(data, node, command);
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
                    foreach (var pair in tab.Context.Tables[data.RequestedType].Columns.Values)
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

        public async ValueTask<object> SolveDeleteQuery(List<ExpressionNode> expr, CancellationToken token, IRelation tab, DbTransaction transaction)
        {
            SqlSelectorData data = sqlBuilder.BuildSelectorData(expr, new SqlSelectorData());
            await using DbCommand command = tab.Context.Connection.CreateCommand();
            command.Transaction = transaction;
            sqlBuilder.WriteSqlDelete(data, tab, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            await command.ExecuteNonQueryAsync(token);
            return null;
        }

        public async ValueTask<object> SolveCreateQuery(List<ExpressionNode> pn, CancellationToken token, IRelation tab, DbTransaction transaction)
        {
            CreateExpressionNode create = pn.Last() as CreateExpressionNode;
            await using (DbCommand command = tab.Context.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                sqlBuilder.WriteSqlCreate(create, tab, command);
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