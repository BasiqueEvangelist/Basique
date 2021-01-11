using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using Basique.Modeling;
using System.Diagnostics;

namespace Basique.Flattening
{
    public static class ToplevelExpressionFlattener
    {
        public static ExpressionNode Parse(Expression expr)
        {
            while (expr.CanReduce)
                expr = expr.ReduceAndCheck();
            if (expr is ConstantExpression con)
                return new FinalExpressionNode(con.Value as IRelation);
            else if (expr is MethodCallExpression call)
            {
                if (call.Method.GetGenericMethodDefinition() == KnownMethods.Where)
                {
                    if (call.Arguments[1] is not UnaryExpression quote)
                        throw new NotImplementedException();
                    if (!(quote.NodeType == ExpressionType.Quote))
                        throw new NotImplementedException();
                    if (quote.Operand is not LambdaExpression lambda)
                        throw new NotImplementedException();

                    return new WhereExpressionNode() { Condition = PredicateFlattener.Flatten(lambda.Body, lambda.Parameters), Parent = Parse(call.Arguments[0]) };
                }
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.OrderBy
                      || call.Method.GetGenericMethodDefinition() == KnownMethods.OrderByDescending)
                {
                    if (call.Arguments[1] is not UnaryExpression quote)
                        throw new NotImplementedException();
                    if (!(quote.NodeType == ExpressionType.Quote))
                        throw new NotImplementedException();
                    if (quote.Operand is not LambdaExpression lambda)
                        throw new NotImplementedException();

                    bool isDescending = call.Method.GetGenericMethodDefinition() == KnownMethods.OrderByDescending;

                    return new OrderByExpressionNode() { Key = PredicateFlattener.Flatten(lambda.Body, lambda.Parameters), Descending = isDescending, Parent = Parse(call.Arguments[0]) };
                }
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.ThenBy
                      || call.Method.GetGenericMethodDefinition() == KnownMethods.ThenByDescending)
                {
                    if (call.Arguments[1] is not UnaryExpression quote)
                        throw new NotImplementedException();
                    if (!(quote.NodeType == ExpressionType.Quote))
                        throw new NotImplementedException();
                    if (quote.Operand is not LambdaExpression lambda)
                        throw new NotImplementedException();

                    bool isDescending = call.Method.GetGenericMethodDefinition() == KnownMethods.ThenByDescending;

                    return new ThenByExpressionNode() { Key = PredicateFlattener.Flatten(lambda.Body, lambda.Parameters), Descending = isDescending, Parent = Parse(call.Arguments[0]) };
                }
                else if (KnownMethods.PullSingleVariants.Contains(call.Method.GetGenericMethodDefinition()))
                {
                    var method = call.Method.GetGenericMethodDefinition();

                    PullSingleExpressionNode pullSingle = new() { Parent = Parse(call.Arguments[0]) };

                    if (KnownMethods.PullSinglePredicated.Contains(method))
                    {
                        if (call.Arguments[1] is not UnaryExpression quote)
                            throw new NotImplementedException();
                        if (!(quote.NodeType == ExpressionType.Quote))
                            throw new NotImplementedException();
                        if (quote.Operand is not LambdaExpression lambda)
                            throw new NotImplementedException();
                        pullSingle.By = PredicateFlattener.Flatten(lambda.Body, lambda.Parameters);
                    }
                    else
                        pullSingle.By = null;
                    pullSingle.IncludeDefault = KnownMethods.PullSingleDefault.Contains(method);
                    if (KnownMethods.PullSingleFirst.Contains(method))
                        pullSingle.Type = PullSingleExpressionNode.PullType.First;
                    else if (KnownMethods.PullSingleSingle.Contains(method))
                        pullSingle.Type = PullSingleExpressionNode.PullType.Single;
                    else
                        throw new NotImplementedException();

                    return pullSingle;
                }
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.Take)
                    return new LimitExpressionNode() { Count = (int)(call.Arguments[1] as ConstantExpression).Value, Parent = Parse(call.Arguments[0]) };
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.WithTransaction)
                    return new TransactionExpressionNode() { Transaction = (call.Arguments[1] as ConstantExpression).Value as BasiqueTransaction, Parent = Parse(call.Arguments[0]) };
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.ToListAsync)
                    return new PullExpressionNode() { Type = PullExpressionNode.PullType.List, Parent = Parse(call.Arguments[0]) };
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.ToArrayAsync)
                    return new PullExpressionNode() { Type = PullExpressionNode.PullType.Array, Parent = Parse(call.Arguments[0]) };
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.CreateAsyncInternal)
                {
                    if (call.Arguments[1] is not UnaryExpression quote)
                        throw new NotImplementedException();
                    if (!(quote.NodeType == ExpressionType.Quote))
                        throw new NotImplementedException();
                    if (quote.Operand is not LambdaExpression lambda)
                        throw new NotImplementedException();

                    return new CreateExpressionNode()
                    {
                        OfType = call.Method.GetGenericArguments()[0],
                        InitList = InitList.GetInitList(lambda.Parameters, lambda.Body).ToDictionary(x => x.Key, x => x.Value),
                        Parent = Parse(call.Arguments[0])
                    };
                }
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.Commit)
                    return new UpdateExpressionNode() { Context = (call.Arguments[1] as ConstantExpression).Value as UpdateContext, Parent = Parse(call.Arguments[0]) };
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.DeleteAsync)
                    return new DeleteExpressionNode() { Parent = Parse(call.Arguments[0]) };
                else if (call.Method.GetGenericMethodDefinition() == KnownMethods.Select)
                {
                    if (call.Arguments[1] is not UnaryExpression quote)
                        throw new NotImplementedException();
                    if (!(quote.NodeType == ExpressionType.Quote))
                        throw new NotImplementedException();
                    if (quote.Operand is not LambdaExpression lambda)
                        throw new NotImplementedException();

                    return new SelectExpressionNode()
                    {
                        To = call.Method.GetGenericArguments()[1],
                        Via = lambda,
                        Parent = Parse(call.Arguments[0])
                    };
                }
                else if (KnownMethods.CountVariants.Contains(call.Method.GetGenericMethodDefinition()))
                {
                    var method = call.Method.GetGenericMethodDefinition();

                    var count = new CountExpressionNode()
                    {
                        Type = method switch
                        {
                            _ when KnownMethods.LongCountVariants.Contains(method) => CountExpressionNode.CountType.LongCount,
                            _ when KnownMethods.CountCount.Contains(method) => CountExpressionNode.CountType.Count,
                            _ when KnownMethods.CountAny.Contains(method) => CountExpressionNode.CountType.Any,
                            _ when method == KnownMethods.All => CountExpressionNode.CountType.All,

                            _ => throw new NotImplementedException()
                        },
                        Parent = Parse(call.Arguments[0])
                    };
                    if (KnownMethods.CountPredicates.Contains(method))
                    {
                        if (call.Arguments[1] is not UnaryExpression quote)
                            throw new NotImplementedException();
                        if (!(quote.NodeType == ExpressionType.Quote))
                            throw new NotImplementedException();
                        if (quote.Operand is not LambdaExpression lambda)
                            throw new NotImplementedException();
                        count.Predicate = PredicateFlattener.Flatten(lambda.Body, lambda.Parameters);
                    }
                    return count;
                }
                else
                    throw new NotImplementedException();
            }
            else
                throw new NotImplementedException();
        }
        public static List<ExpressionNode> ParseAndFlatten(Expression expr)
        {
            ExpressionNode root = Parse(expr);
            List<ExpressionNode> list = new();
            list.Add(root);
            for (ExpressionNode cur = root; cur != cur.Parent; cur = cur.Parent)
            {
                list.Add(cur.Parent);
            }
            list.Reverse();
            return list;
        }
    }
}