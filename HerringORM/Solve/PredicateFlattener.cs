using System.Runtime.CompilerServices;
using System.ComponentModel;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HerringORM.Solve
{
    public static class PredicateFlattener
    {
        public static FlatPredicateNode Flatten(Expression expr)
        {
            if (expr is ConstantExpression con)
                return new ConstantPredicate() { Of = con.Type, Data = con.Value };
            else if (expr is BinaryExpression bin)
            {
                var pred = new BinaryPredicate() { Left = Flatten(bin.Left), Right = Flatten(bin.Right) };
                if (bin.NodeType == ExpressionType.Equal)
                    pred.Type = BinaryPredicateType.Equal;
                else if (bin.NodeType == ExpressionType.NotEqual)
                    pred.Type = BinaryPredicateType.NotEqual;
                else if (bin.NodeType == ExpressionType.LessThan)
                    pred.Type = BinaryPredicateType.Less;
                else if (bin.NodeType == ExpressionType.AndAlso)
                    pred.Type = BinaryPredicateType.AndAlso;
                else if (bin.NodeType == ExpressionType.OrElse)
                    pred.Type = BinaryPredicateType.OrElse;
                else if (bin.NodeType == ExpressionType.ExclusiveOr)
                    pred.Type = BinaryPredicateType.ExclusiveOr;
                else if (bin.NodeType == ExpressionType.GreaterThan)
                    pred.Type = BinaryPredicateType.Greater;
                else
                    throw new NotImplementedException();
                return pred;
            }
            else if (expr is ConditionalExpression cond)
                return new TernaryPredicate() { Condition = Flatten(cond.Test), OnTrue = Flatten(cond.IfTrue), OnFalse = Flatten(cond.IfFalse) };
            else if (expr is MemberExpression mem)
            {
                if (!(mem.Member is FieldInfo field))
                    throw new NotImplementedException();
                return new SubPredicate() { Field = field, From = Flatten(mem.Expression) };
            }
            else if (expr is ParameterExpression param)
                return new ContextPredicate() { Of = param.Type };
            else
                throw new NotImplementedException();
        }
    }
}