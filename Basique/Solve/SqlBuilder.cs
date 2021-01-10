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
                prefix = WriteSqlPredicate(data.Relation, data.Columns, data.Where, cmd, prefix++, s);
            }
            cmd.CommandText = s.ToString();
        }

        public static void WriteSqlPullSingle(SqlSelectorData data, PullSingleExpressionNode node, DbCommand cmd)
        {
            StringBuilder s = new StringBuilder();
            int prefix = 0;
            s.Append("select ");
            s.AppendJoin(", ", data.Columns.WalkColumns().Select(x => $"{x.Value.From.NamedAs}.{x.Value.Column.Name} as {x.Value.NamedAs}"));
            s.Append(" from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Columns, data.Where, cmd, prefix++, s);
            }
            if (data.OrderBy.Count > 0)
            {
                s.Append(" order by ");
                for (int i = 0; i < data.OrderBy.Count; i++)
                {
                    prefix = WriteSqlPredicate(data.Relation, data.Columns, data.OrderBy[i].Key, cmd, prefix++, s);
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
            s.Append("select ");
            s.AppendJoin(", ", data.Columns.WalkColumns().Select(x => $"{x.Value.From.NamedAs}.{x.Value.Column.Name} as {x.Value.NamedAs}"));
            s.Append(" from ");
            s.Append(data.Relation.Name);
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Columns, data.Where, cmd, prefix, s);
            }

            if (data.OrderBy.Count > 0)
            {
                s.Append(" order by ");
                for (int i = 0; i < data.OrderBy.Count; i++)
                {
                    prefix = WriteSqlPredicate(data.Relation, data.Columns, data.OrderBy[i].Key, cmd, prefix++, s);
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
            for (int i = 0; i < data.UpdateContext.Data.Count; i++)
            {
                var part = data.UpdateContext.Data[i];
                var column = data.Columns.GetByPath(part.field).AssertColumn();
                s.Append($"{column.Column.Name} = ");
                prefix = WriteSqlPredicate(data.Relation, data.Columns, part.factory, cmd, prefix, s);
                if (i != data.UpdateContext.Data.Count - 1)
                    s.Append(", ");
            }
            if (data.Where != null)
            {
                s.Append(" where ");
                prefix = WriteSqlPredicate(data.Relation, data.Columns, data.Where, cmd, prefix++, s);
            }

            cmd.CommandText = s.ToString();
        }

        private static int WriteSqlPredicate(IRelation tab, ColumnSet set, FlatPredicateNode node, DbCommand cmd, int prefix, StringBuilder into)
        {
            if (node is BinaryPredicate bin)
            {
                into.Append("(");
                prefix = WriteSqlPredicate(tab, set, bin.Left, cmd, prefix, into);
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
                prefix = WriteSqlPredicate(tab, set, bin.Right, cmd, prefix, into);
                into.Append(")");
            }
            else if (node is UnaryPredicate una)
            {
                if (una.Type == UnaryPredicateType.Not)
                    into.Append("NOT ");
                else
                    throw new NotImplementedException();

                into.Append("(");
                prefix = WriteSqlPredicate(tab, set, una.Operand, cmd, prefix, into);
                into.Append(")");
            }
            else if (node is TernaryPredicate tern)
            {
                into.Append("CASE WHEN (");
                prefix = WriteSqlPredicate(tab, set, tern.Condition, cmd, prefix, into);
                into.Append(") THEN (");
                prefix = WriteSqlPredicate(tab, set, tern.OnTrue, cmd, prefix, into);
                into.Append(") ELSE (");
                prefix = WriteSqlPredicate(tab, set, tern.OnFalse, cmd, prefix, into);
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
                    var column = set.GetByPath(sub.Path).AssertColumn();
                    into.Append($"{column.From.NamedAs}.{column.Column.Name}");
                }
                else
                    throw new NotImplementedException(); // Joins and .Select() will come later.
            }
            return prefix;
        }

        public static void WriteSqlCreate(CreateExpressionNode create, IRelation tab, DbCommand command)
        {
            var set = LinqVM.BuildSimpleColumnSet(tab);

            StringBuilder s = new StringBuilder();
            s.Append("insert into ");
            s.Append(tab.Name);
            s.Append(" (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => set[x.Member].AssertColumn().Column.Name));
            s.Append(") values (");
            s.AppendJoin(",", create.Factory.Bindings.OfType<MemberAssignment>().Select(x => "@" + set[x.Member].AssertColumn().Column.Name));
            s.Append(");");
            command.CommandText = s.ToString();
            foreach (var assign in create.Factory.Bindings.OfType<MemberAssignment>())
            {
                var param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@" + set[assign.Member].AssertColumn().Column.Name;
                param.Value = Expression.Lambda(assign.Expression).Compile(false).DynamicInvoke();
                command.Parameters.Add(param);
            }
        }
    }
}