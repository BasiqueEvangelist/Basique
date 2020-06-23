using System.Data;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System;
using System.Collections.Generic;
using HerringORM.Modeling;
using System.Data.Common;

namespace HerringORM.Solve
{
    public static class SqlBuilder
    {
        public static SqlSelectData BuildSelectData(List<ExpressionNode> nodes)
        {
            SqlSelectData data = new SqlSelectData();
            data.FromTable = ((FinalExpressionNode)nodes[0]).Table;
            Type currentType = data.FromTable.ElementType;
            foreach (var node in nodes)
            {
                if (node is FinalExpressionNode)
                    continue;
                else if (node is PullExpressionNode)
                    continue; // Not our job.
                else if (node is WhereExpressionNode whereexpr)
                {
                    data.Rules.Add(new WhereRule()
                    {
                        Condition = whereexpr.Condition
                    });
                }
            }
            data.RequestedType = currentType;
            return data;
        }

        public static void WriteSqlSelect(SqlSelectData data, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("select ");
            s.AppendJoin(", ", data.RequestedType.GetTypeInfo().GetFields().Select(x => x.Name.ToLower()));
            s.Append(" from ");
            s.Append(data.FromTable.Name);
            foreach (var rule in data.Rules)
            {
                if (rule is WhereRule where)
                {
                    s.Append(" where ");
                    prefix = WriteSqlPredicate(data.FromTable, where.Condition, cmd, prefix++, s);
                }
                else if (rule is JoinRule join)
                {
                    s.Append($" {join.Type} join {join.To} on {join.On}");
                }
            }

            cmd.CommandText = s.ToString();
        }

        private static int WriteSqlPredicate(ITable tab, FlatPredicateNode node, DbCommand cmd, int prefix, StringBuilder into)
        {
            if (node is BinaryPredicate bin)
            {
                prefix = WriteSqlPredicate(tab, bin.Left, cmd, prefix, into);
                if (bin.Type == BinaryPredicateType.Equal)
                    into.Append(" = ");
                else if (bin.Type == BinaryPredicateType.NotEqual)
                    into.Append(" <> ");
                else if (bin.Type == BinaryPredicateType.Greater)
                    into.Append(" > ");
                else if (bin.Type == BinaryPredicateType.Less)
                    into.Append(" < ");
                else
                    throw new NotImplementedException();
                prefix = WriteSqlPredicate(tab, bin.Right, cmd, prefix, into);
            }
            else if (node is ConstantPredicate con)
            {
                string name = $"@constant{prefix++}";
                into.Append(name);
                var param = cmd.CreateParameter();
                param.ParameterName = name;
                param.Direction = ParameterDirection.Input;
                param.Value = con.Data;
                cmd.Parameters.Add(param);
            }
            else if (node is SubPredicate sub)
            {
                if (sub.From is ContextPredicate ctx)
                {
                    into.Append($"{tab.Name}.{sub.Field.Name.ToLower()}");
                }
                else
                    throw new NotImplementedException(); // Joins and .Select() will come later.
            }
            return prefix;
        }

        public static void WriteSqlCreate(CreateExpressionNode create, ITable tab, DbCommand command)
        {
            StringBuilder s = new StringBuilder();
            s.Append("insert into ");
            s.Append(tab.Name);
            s.Append(" (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => x.Member.Name.ToLower()));
            s.Append(") values (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => "@" + x.Member.Name.ToLower()));
            s.Append(");");
            command.CommandText = s.ToString();
            foreach (var assign in create.Factory.Bindings.OfType<MemberAssignment>())
            {
                var param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@" + assign.Member.Name.ToLower();
                param.Value = Expression.Lambda(assign.Expression).Compile(false).DynamicInvoke();
                command.Parameters.Add(param);
            }
        }
    }
    public class SqlSelectData
    {
        public List<SqlRule> Rules = new List<SqlRule>();
        public Type RequestedType;
        public ITable FromTable;
    }

    public abstract class SqlRule { }
    public class WhereRule : SqlRule
    {
        public FlatPredicateNode Condition;
    }
    public class JoinRule : SqlRule
    {
        public string Type;
        public string To;
        public string On;
    }
}