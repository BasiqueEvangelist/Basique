using System.Data;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System;
using System.Collections.Generic;
using Basique.Modeling;
using System.Data.Common;
using Basique.Services;
using Basique.Flattening;

namespace Basique.Solve
{
    public static class SqlBuilder
    {
        public static void WriteSqlDelete(SqlSelectorData data, IRelation tab, DbCommand cmd)
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
            cmd.CommandText = s.ToString();
        }

        public static void WriteSqlPullSingle(SqlSelectorData data, PullSingleExpressionNode node, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("select ");
            s.AppendJoin(", ", data.Relation.Schema.Tables[data.Relation.ElementType].Columns.Select(x => x.Value.Name));
            s.Append(" from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix++, s);
            }
            if (data.OrderBy.Count > 0)
            {
                s.Append(" order by ");
                for (int i = 0; i < data.OrderBy.Count; i++)
                {
                    prefix = WriteSqlPredicate(data.Relation, data.OrderBy[i].Key, cmd, prefix++, s);
                    s.Append(data.OrderBy[i].Descending ? " desc" : " asc");
                    if (i != data.OrderBy.Count - 1)
                    {
                        s.Append(',');
                    }
                }
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

        public static void WriteSqlSelect(SqlSelectorData data, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            var tableInfo = data.Relation.Schema.Tables[data.Relation.ElementType];
            s.Append("select ");
            s.AppendJoin(", ", tableInfo.Columns.Select(x => $"{tableInfo.Name}.{x.Value.Name}"));
            s.Append(" from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix, s);
            }

            if (data.OrderBy.Count > 0)
            {
                s.Append(" order by ");
                for (int i = 0; i < data.OrderBy.Count; i++)
                {
                    prefix = WriteSqlPredicate(data.Relation, data.OrderBy[i].Key, cmd, prefix++, s);
                    s.Append(data.OrderBy[i].Descending ? " desc" : " asc");
                    if (i != data.OrderBy.Count - 1)
                    {
                        s.Append(',');
                    }
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
            s.Append(data.Relation.Name);
            s.Append(" set ");
            for (int i = 0; i < data.Context.Data.Count; i++)
            {
                var part = data.Context.Data[i];
                s.Append($"{data.Relation.Schema.Tables[data.Relation.ElementType].Columns[part.field.ToString()].Name} = ");
                prefix = WriteSqlPredicate(data.Relation, part.factory, cmd, prefix, s);
                if (i != data.Context.Data.Count - 1)
                    s.Append(", ");
            }
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Where, cmd, prefix++, s);
            }

            cmd.CommandText = s.ToString();
        }

        private static int WriteSqlPredicate(IRelation tab, FlatPredicateNode node, DbCommand cmd, int prefix, StringBuilder into)
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
                else if (bin.Type == BinaryPredicateType.GreaterOrEqual)
                    into.Append(" >= ");
                else if (bin.Type == BinaryPredicateType.LessOrEqual)
                    into.Append(" <= ");
                else if (bin.Type == BinaryPredicateType.Add)
                    into.Append(" + ");
                else if (bin.Type == BinaryPredicateType.Subtract)
                    into.Append(" - ");
                else if (bin.Type == BinaryPredicateType.Multiply)
                    into.Append(" * ");
                else if (bin.Type == BinaryPredicateType.Divide)
                    into.Append(" / ");
                else if (bin.Type == BinaryPredicateType.Modulo)
                    into.Append(" % ");
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
                    into.Append($"{tab.Name}.{tab.Schema.Tables[sub.Path.Members[0].DeclaringType].Columns[sub.Path.ToString()].Name}");
                }
                else
                    throw new NotImplementedException(); // Joins and .Select() will come later.
            }
            return prefix;
        }

        public static void WriteSqlCreate(CreateExpressionNode create, IRelation tab, DbCommand command)
        {
            StringBuilder s = new StringBuilder();
            s.Append("insert into ");
            s.Append(tab.Name);
            s.Append(" (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => tab.Schema.Tables[x.Member.DeclaringType].Columns[new MemberPath(x.Member).ToString()].Name));
            s.Append(") values (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => "@" + tab.Schema.Tables[x.Member.DeclaringType].Columns[new MemberPath(x.Member).ToString()].Name));
            s.Append(");");
            command.CommandText = s.ToString();
            foreach (var assign in create.Factory.Bindings.OfType<MemberAssignment>())
            {
                var param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@" + tab.Schema.Tables[assign.Member.DeclaringType].Columns[new MemberPath(assign.Member).ToString()].Name;
                param.Value = Expression.Lambda(assign.Expression).Compile(false).DynamicInvoke();
                command.Parameters.Add(param);
            }
        }
    }
}