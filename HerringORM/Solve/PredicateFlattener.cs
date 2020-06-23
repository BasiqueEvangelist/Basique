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
                if (bin.NodeType == ExpressionType.Equal)
                    return new EqualPredicate() { Left = Flatten(bin.Left), Right = Flatten(bin.Right) };
                else
                    throw new NotImplementedException();
            }
            else if (expr is MemberExpression mem)
            {
                if (!(mem.Member is FieldInfo field))
                    throw new NotImplementedException();
                return new SubPredicate() { Field = field, From = Flatten(mem.Expression) };
            }
            else if (expr is ParameterExpression param)
                return new VariablePredicate() { Of = param.Type };
            else
                throw new NotImplementedException();
        }
    }
}