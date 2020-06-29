using System.Runtime.CompilerServices;
using System.ComponentModel;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Basique.Solve
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
                else if (bin.NodeType == ExpressionType.LessThanOrEqual)
                    pred.Type = BinaryPredicateType.LessOrEqual;
                else if (bin.NodeType == ExpressionType.GreaterThanOrEqual)
                    pred.Type = BinaryPredicateType.GreaterOrEqual;
                else if (bin.NodeType == ExpressionType.Add)
                    pred.Type = BinaryPredicateType.Add;
                else if (bin.NodeType == ExpressionType.Subtract)
                    pred.Type = BinaryPredicateType.Subtract;
                else if (bin.NodeType == ExpressionType.Multiply)
                    pred.Type = BinaryPredicateType.Multiply;
                else if (bin.NodeType == ExpressionType.Divide)
                    pred.Type = BinaryPredicateType.Divide;
                else if (bin.NodeType == ExpressionType.Modulo)
                    pred.Type = BinaryPredicateType.Modulo;
                else
                    throw new NotImplementedException();
                return pred;
            }
            else if (expr is UnaryExpression una)
            {
                var pred = new UnaryPredicate() { Operand = Flatten(una.Operand) };
                if (una.NodeType == ExpressionType.Not)
                    pred.Type = UnaryPredicateType.Not;
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
                var pack = MemberPath.Create(mem);
                return new SubPredicate() { Path = pack.Path, From = Flatten(pack.Final) };
            }
            else if (expr is ParameterExpression param)
                return new ContextPredicate() { Of = param.Type };
            else
                throw new NotImplementedException();
        }
    }
}