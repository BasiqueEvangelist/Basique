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
    public class SqlBuilder
    {
        public SqlSelectorData BuildSelectorData(List<ExpressionNode> nodes, SqlSelectorData data)
        {
            data.Relation = ((FinalExpressionNode)nodes[0]).Table;
            Type currentType = data.Relation.ElementType;
            foreach (var node in nodes)
            {
                if (node is FinalExpressionNode)
                    continue;
                else if (node is PullExpressionNode
                      || node is UpdateExpressionNode
                      || node is DeleteExpressionNode)
                    continue; // Not our job.
                else if (node is PullSingleExpressionNode pull)
                {
                    if (pull.By != null)
                    {
                        if (data.Where == null)
                            data.Where = pull.By;
                        else
                            data.Where = new BinaryPredicate() { Left = data.Where, Right = pull.By, Type = BinaryPredicateType.AndAlso };
                    }
                }
                else if (node is WhereExpressionNode whereexpr)
                {
                    if (data.Where == null)
                        data.Where = whereexpr.Condition;
                    else
                        data.Where = new BinaryPredicate() { Left = data.Where, Right = whereexpr.Condition, Type = BinaryPredicateType.AndAlso };
                }
                else if (node is OrderByExpressionNode orderby)
                    data.OrderBy = orderby;
                else if (node is LimitExpressionNode limit)
                    if ((data.Limit ?? int.MaxValue) > limit.Count)
                        data.Limit = limit.Count;
                    else
                        throw new NotImplementedException();
            }
            data.RequestedType = currentType;
            return data;
        }

        public SqlUpdateData BuildUpdateData(List<ExpressionNode> nodes)
        {
            SqlUpdateData data = new SqlUpdateData();
            BuildSelectorData(nodes, data);
            data.Context = (nodes[^1] as UpdateExpressionNode).Context;
            return data;
        }

        public void WriteSqlDelete(SqlSelectorData data, IRelation tab, DbCommand cmd)
        {
            if (data.Limit != null)
                throw new InvalidOperationException("Limits on Delete not allowed.");
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("delete from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix++, s);
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

        public void WriteSqlPullSingle(SqlSelectorData data, PullSingleExpressionNode node, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("select ");
            s.AppendJoin(", ", data.Relation.Context.Tables[data.RequestedType].Columns.Select(x => x.Value.Name));
            s.Append(" from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix++, s);
            }
            foreach (var rule in data.Rules)
            {
                if (rule is JoinRule join)
                {
                    s.Append($" {join.Type} join {join.To} on {join.On}");
                }
            }
            if (data.OrderBy != null)
            {
                s.Append(" order by ");
                prefix = WriteSqlPredicate(data.Relation, data.OrderBy.Key, cmd, prefix++, s);
                s.Append(data.OrderBy.Descending ? " desc" : " asc");
            }
            int limitNeeded = node.Type == PullSingleExpressionNode.PullType.First ? 1 : 2;
            if (data.Limit == null)
            {
                data.Limit = limitNeeded;
                data.Limit = data.Limit > limitNeeded ? limitNeeded : data.Limit;
            }
            s.Append($" limit {data.Limit}");
            cmd.CommandText = s.ToString();
        }

        public void WriteSqlSelect(SqlSelectorData data, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            var tableInfo = data.Relation.Context.Tables[data.RequestedType];
            s.Append("select ");
            s.AppendJoin(", ", tableInfo.Columns.Select(x => $"{tableInfo.Name}.{x.Value.Name}"));
            s.Append(" from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix, s);
            }

            if (data.OrderBy != null)
            {
                s.Append(" order by ");
                prefix = WriteSqlPredicate(data.Relation, data.OrderBy.Key, cmd, prefix, s);
                s.Append(data.OrderBy.Descending ? " desc" : " asc");
            }

            if (data.Limit != null)
                s.Append($" limit {data.Limit}");

            cmd.CommandText = s.ToString();
        }

        public void WriteSqlUpdate(SqlUpdateData data, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("update ");
            s.Append(data.Relation.Name);
            s.Append(" set ");
            for (int i = 0; i < data.Context.Data.Count; i++)
            {
                var part = data.Context.Data[i];
                s.Append($"{data.Relation.Context.Tables[data.RequestedType].Columns[part.field.ToString()].Name} = ");
                prefix = WriteSqlPredicate(data.Relation, part.factory, cmd, prefix, s);
                if (i != data.Context.Data.Count - 1)
                    s.Append(", ");
            }
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix++, s);
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

        private int WriteSqlPredicate(IRelation tab, FlatPredicateNode node, DbCommand cmd, int prefix, StringBuilder into)
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
            else if (node is UnaryPredicate una)
            {
                if (una.Type == UnaryPredicateType.Not)
                    into.Append("NOT ");
                else
                    throw new NotImplementedException();

                into.Append("(");
                prefix = WriteSqlPredicate(tab, una.Operand, cmd, prefix, into);
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
                    into.Append($"{tab.Name}.{tab.Context.Tables[sub.Path.Members[0].DeclaringType].Columns[sub.Path.ToString()].Name}");
                }
                else
                    throw new NotImplementedException(); // Joins and .Select() will come later.
            }
            return prefix;
        }

        public void WriteSqlCreate(CreateExpressionNode create, IRelation tab, DbCommand command)
        {
            StringBuilder s = new StringBuilder();
            s.Append("insert into ");
            s.Append(tab.Name);
            s.Append(" (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => tab.Context.Tables[x.Member.DeclaringType].Columns[new MemberPath(x.Member).ToString()].Name));
            s.Append(") values (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => "@" + tab.Context.Tables[x.Member.DeclaringType].Columns[new MemberPath(x.Member).ToString()].Name));
            s.Append(");");
            command.CommandText = s.ToString();
            foreach (var assign in create.Factory.Bindings.OfType<MemberAssignment>())
            {
                var param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@" + tab.Context.Tables[assign.Member.DeclaringType].Columns[new MemberPath(assign.Member).ToString()].Name;
                param.Value = Expression.Lambda(assign.Expression).Compile(false).DynamicInvoke();
                command.Parameters.Add(param);
            }
        }
    }
    public class SqlUpdateData : SqlSelectorData
    {
        public UpdateContext Context;
    }

    public class SqlSelectorData
    {
        public int? Limit;
        public Type RequestedType;
        public List<SqlRule> Rules = new List<SqlRule>();
        public OrderByExpressionNode OrderBy;
        public FlatPredicateNode Where;
        public IRelation Relation;
    }

    public abstract class SqlRule { }
    public class JoinRule : SqlRule
    {
        public string Type;
        public string To;
        public string On;
    }
}