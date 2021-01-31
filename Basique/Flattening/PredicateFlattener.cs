using System;
using System.Linq.Expressions;
using Basique.Services;
using System.Collections.Generic;
using System.Linq;

namespace Basique.Flattening
{
    public static class PredicateFlattener
    {
        public static FlatPredicateNode Flatten(Expression expr, IList<ParameterExpression> parameters)
        {
            if (expr is ConstantExpression con)
                return new ConstantPredicate() { Of = con.Type, Data = con.Value };
            else if (expr is BinaryExpression bin)
            {
                var pred = new BinaryPredicate() { Left = Flatten(bin.Left, parameters), Right = Flatten(bin.Right, parameters) };
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
                var pred = new UnaryPredicate() { Operand = Flatten(una.Operand, parameters) };
                if (una.NodeType == ExpressionType.Not)
                    pred.Type = UnaryPredicateType.Not;
                else if (una.NodeType == ExpressionType.Convert)
                {
                    if (una.Type.GetGenericTypeDefinition() == typeof(Nullable<>)
                     && una.Type.GetGenericArguments()[0].AssemblyQualifiedName == una.Operand.Type.AssemblyQualifiedName)
                        return Flatten(una.Operand, parameters);
                }
                else
                    throw new NotImplementedException();
                return pred;
            }
            else if (expr is ConditionalExpression cond)
                return new TernaryPredicate() { Condition = Flatten(cond.Test, parameters), OnTrue = Flatten(cond.IfTrue, parameters), OnFalse = Flatten(cond.IfFalse, parameters) };
            else if (expr is MemberExpression mem)
            {
                var (path, final) = MemberPath.Create(mem);
                return new SubPredicate() { Path = path, From = final == null ? null : Flatten(final, parameters) };
            }
            else if (expr is ParameterExpression param)
                return new ContextPredicate() { Of = param.Type, ContextId = parameters.IndexOf(param) };
            else if (expr is MethodCallExpression call)
                return new CallPredicate()
                {
                    Instance = call.Method.IsStatic ? null : Flatten(call.Object, parameters),
                    Method = call.Method,
                    Arguments = call.Arguments.Select(x => Flatten(x, parameters)).ToArray()
                };
            else
                throw new NotImplementedException();
        }
    }
}