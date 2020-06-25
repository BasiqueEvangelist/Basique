using System.Data;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System;
using System.Collections.Generic;
using Basique.Modeling;
using System.Data.Common;

namespace Basique.Solve
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
                    if (data.Where == null)
                        data.Where = whereexpr.Condition;
                    else
                        data.Where = new BinaryPredicate() { Left = data.Where, Right = whereexpr.Condition, Type = BinaryPredicateType.AndAlso };
                }
                else if (node is LimitExpressionNode limit)
                    if ((data.Limit ?? int.MaxValue) > limit.Count)
                        data.Limit = limit.Count;
                    else
                        throw new NotImplementedException();
            }
            data.RequestedType = currentType;
            return data;
        }

        public static SqlUpdateData BuildUpdateData(List<ExpressionNode> nodes)
        {
            SqlUpdateData data = new SqlUpdateData();
            data.FromTable = ((FinalExpressionNode)nodes[0]).Table;
            data.Context = ((UpdateExpressionNode)nodes[^1]).Context;
            Type currentType = data.FromTable.ElementType;
            foreach (var node in nodes)
            {
                if (node is FinalExpressionNode)
                    continue;
                else if (node is UpdateExpressionNode)
                    continue; // Not our job.
                else if (node is WhereExpressionNode whereexpr)
                {
                    if (data.Where == null)
                        data.Where = whereexpr.Condition;
                    else
                        data.Where = new BinaryPredicate() { Left = data.Where, Right = whereexpr.Condition, Type = BinaryPredicateType.AndAlso };
                }
                else
                    throw new NotImplementedException();
            }
            return data;
        }

        public static SqlDeleteData BuildDeleteData(List<ExpressionNode> nodes)
        {
            SqlDeleteData data = new SqlDeleteData();
            data.FromTable = ((FinalExpressionNode)nodes[0]).Table;
            Type currentType = data.FromTable.ElementType;
            foreach (var node in nodes)
            {
                if (node is FinalExpressionNode)
                    continue;
                else if (node is DeleteExpressionNode)
                    continue; // Not our job.
                else if (node is WhereExpressionNode whereexpr)
                {
                    if (data.Where == null)
                        data.Where = whereexpr.Condition;
                    else
                        data.Where = new BinaryPredicate() { Left = data.Where, Right = whereexpr.Condition, Type = BinaryPredicateType.AndAlso };
                }
                else
                    throw new NotImplementedException();
            }
            return data;
        }

        public static void WriteSqlDelete<T>(SqlDeleteData data, Table<T> tab, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("delete from ");
            s.Append(data.FromTable.Name);
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.FromTable, data.Where, cmd, prefix++, s);
            }
            foreach (var rule in data.Rules)
            {
                if (rule is JoinRule join)
                {
                    s.Append($" {join.Type} join {join.To} on {join.On}");
                }
            }
            cmd.CommandText = s.ToString();
        }

        public static void WriteSqlSelect(SqlSelectData data, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("select ");
            s.AppendJoin(", ", data.RequestedType.GetTypeInfo().GetFields().Select(x => x.Name.ToLower()));
            s.Append(" from ");
            s.Append(data.FromTable.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.FromTable, data.Where, cmd, prefix++, s);
            }
            foreach (var rule in data.Rules)
            {
                if (rule is JoinRule join)
                {
                    s.Append($" {join.Type} join {join.To} on {join.On}");
                }
            }
            if (data.Limit != null)
                s.Append($" limit {data.Limit}");
            cmd.CommandText = s.ToString();
        }

        public static void WriteSqlUpdate(SqlUpdateData data, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("update ");
            s.Append(data.FromTable.Name);
            s.Append(" set ");
            for (int i = 0; i < data.Context.Data.Count; i++)
            {
                var part = data.Context.Data[i];
                s.Append($"{part.field.Name.ToLower()} = ");
                prefix = WriteSqlPredicate(data.FromTable, part.factory, cmd, prefix, s);
                if (i != data.Context.Data.Count - 1)
                    s.Append(", ");
            }
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.FromTable, data.Where, cmd, prefix++, s);
            }
            foreach (var rule in data.Rules)
            {
                if (rule is JoinRule join)
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
                into.Append("(");
                prefix = WriteSqlPredicate(tab, bin.Left, cmd, prefix, into);
                into.Append(")");
                if (bin.Type == BinaryPredicateType.Equal)
                    into.Append(" = ");
                else if (bin.Type == BinaryPredicateType.NotEqual)
                    into.Append(" <> ");
                else if (bin.Type == BinaryPredicateType.ExclusiveOr)
                    into.Append(" <> ");
                else if (bin.Type == BinaryPredicateType.AndAlso)
                    into.Append(" AND ");
                else if (bin.Type == BinaryPredicateType.OrElse)
                    into.Append(" OR ");
                else if (bin.Type == BinaryPredicateType.Greater)
                    into.Append(" > ");
                else if (bin.Type == BinaryPredicateType.Less)
                    into.Append(" < ");
                else
                    throw new NotImplementedException();
                into.Append("(");
                prefix = WriteSqlPredicate(tab, bin.Right, cmd, prefix, into);
                into.Append(")");
            }
            else if (node is TernaryPredicate tern)
            {
                into.Append("CASE WHEN (");
                prefix = WriteSqlPredicate(tab, tern.Condition, cmd, prefix, into);
                into.Append(") THEN (");
                prefix = WriteSqlPredicate(tab, tern.OnTrue, cmd, prefix, into);
                into.Append(") ELSE (");
                prefix = WriteSqlPredicate(tab, tern.OnFalse, cmd, prefix, into);
                into.Append(") END");
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
        public FlatPredicateNode Where;
        public Type RequestedType;
        public ITable FromTable;
        public int? Limit;
    }

    public class SqlUpdateData
    {
        public List<SqlRule> Rules = new List<SqlRule>();
        public FlatPredicateNode Where;
        public ITable FromTable;
        public UpdateContext Context;
    }

    public class SqlDeleteData
    {
        public List<SqlRule> Rules = new List<SqlRule>();
        public FlatPredicateNode Where;
        public ITable FromTable;
    }

    public abstract class SqlRule { }
    public class JoinRule : SqlRule
    {
        public string Type;
        public string To;
        public string On;
    }
}