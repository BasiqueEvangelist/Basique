using System.Threading;
using System.Linq.Expressions;
using HerringORM.Modeling;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System;
using System.Reflection;
using System.Data;
using System.Linq;
using NLog;

namespace HerringORM.Solve
{
    public static class QuerySolver
    {
        private static Logger LOGGER = LogManager.GetCurrentClassLogger();

        public static async ValueTask<object> SolvePullQuery(List<ExpressionNode> expr, CancellationToken token, ITable table)
        {
            SqlSelectData data = SqlBuilder.BuildSelectData(expr);
            DbCommand command = table.Context.Connection.CreateCommand();
            SqlBuilder.WriteSqlSelect(data, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            var reader = await command.ExecuteReaderAsync(token);
            List<object> res = new List<object>();
            if (reader.HasRows)
                while (await reader.ReadAsync(token))
                {
                    object obj = Activator.CreateInstance(data.RequestedType);
                    foreach (FieldInfo field in data.RequestedType.GetTypeInfo().GetFields())
                    {
                        field.SetValue(obj, Convert.ChangeType(reader.GetValue(field.Name.ToLower()), field.FieldType));
                    }
                    res.Add(obj);
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
            DbCommand command = tab.Context.Connection.CreateCommand();
            SqlBuilder.WriteSqlUpdate(data, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            await command.ExecuteNonQueryAsync();
            return null;
        }

        public static async ValueTask<object> SolveCreateQuery(List<ExpressionNode> pn, CancellationToken token, ITable tab)
        {
            CreateExpressionNode create = pn.Last() as CreateExpressionNode;
            DbCommand command = tab.Context.Connection.CreateCommand();
            SqlBuilder.WriteSqlCreate(create, tab, command);
            LOGGER.Debug("Running SQL: {0}", command.CommandText);
            await command.ExecuteNonQueryAsync();
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