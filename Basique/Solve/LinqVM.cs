using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Basique.Flattening;
using Basique.Modeling;
using Basique.Services;

namespace Basique.Solve
{
    public static class LinqVM
    {
        public static SqlSelectorData BuildSelectorData(List<ExpressionNode> nodes, SqlSelectorData data)
        {
            data.Context = new QueryContext();
            data.Relation = ((FinalExpressionNode)nodes[0]).Table;
            data.Columns = new PathTree<BasiqueColumn>();
            data.Relation.FillSet(data.Columns, data.Context);
            PredicateLinker linker = new(new[] { data.Columns });
            NameAllocator alloc = new(data.Relation.Schema.Logger);
            Type currentType = data.Relation.ElementType;

            while (data.Relation is IJoinRelation join)
            {
                var clause = new JoinClause
                {
                    On = alloc.TransformNode(linker.TransformNode(join.On)),
                    Right = data.Context.GetLogical(join.Right)
                };
                data.Joins.Add(clause);
                data.Relation = join.Left.Original;
            }
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
                            data.Where = alloc.TransformNode(linker.TransformNode(pull.By));
                        else
                            data.Where = new BinaryPredicate() { Left = data.Where, Right = alloc.TransformNode(linker.TransformNode(pull.By)), Type = BinaryPredicateType.AndAlso };
                    }
                }
                else if (node is CountExpressionNode count)
                {
                    if (count.Predicate != null)
                    {
                        var newPredicate = alloc.TransformNode(linker.TransformNode(count.Type == CountExpressionNode.CountType.All ? UnaryPredicate.Not(count.Predicate) : count.Predicate));
                        if (data.Where == null)
                            data.Where = newPredicate;
                        else
                            data.Where = new BinaryPredicate() { Left = data.Where, Right = newPredicate, Type = BinaryPredicateType.AndAlso };
                    }
                }
                else if (node is TransactionExpressionNode trans)
                    data.Transaction = trans.Transaction;
                else if (node is WhereExpressionNode whereexpr)
                {
                    if (data.Where == null)
                        data.Where = alloc.TransformNode(linker.TransformNode(whereexpr.Condition));
                    else
                        data.Where = new BinaryPredicate() { Left = data.Where, Right = alloc.TransformNode(linker.TransformNode(whereexpr.Condition)), Type = BinaryPredicateType.AndAlso };
                }
                else if (node is OrderByExpressionNode orderby)
                {
                    data.OrderBy.Clear();
                    data.OrderBy.Add(new OrderByKey() { Descending = orderby.Descending, Key = alloc.TransformNode(linker.TransformNode(orderby.Key)) });
                }
                else if (node is ThenByExpressionNode thenby)
                    data.OrderBy.Add(new OrderByKey() { Descending = thenby.Descending, Key = alloc.TransformNode(linker.TransformNode(thenby.Key)) });
                else if (node is LimitExpressionNode limit)
                    if ((data.Limit ?? int.MaxValue) > limit.Count)
                        data.Limit = limit.Count;
                    else
                        throw new NotImplementedException();
                else if (node is SelectExpressionNode select)
                {
                    var newSet = new PathTree<BasiqueColumn>();
                    DoSelect(newSet, new PathTree<BasiqueColumn>[] { data.Columns }, select.Via.Parameters, select.Via.Body);
                    currentType = select.To;
                    linker.Contexts = new[] { newSet };
                    data.Columns = newSet;
                }
            }
            data.RequestedType = currentType;

            alloc.NameRelations(data.Columns);
            alloc.NameVariables(data.Columns);

            return data;
        }

        public static SqlUpdateData BuildUpdateData(List<ExpressionNode> nodes)
        {
            SqlUpdateData data = new();
            BuildSelectorData(nodes, data);
            data.UpdateContext = (nodes[^1] as UpdateExpressionNode).Context;
            return data;
        }

        public static PathTree<BasiqueColumn> BuildSimpleColumnSet(IRelation relation)
        {
            var ctx = new QueryContext();
            var set = new PathTree<BasiqueColumn>();
            relation.FillSet(set, ctx);
            var alloc = new NameAllocator(relation.Schema.Logger);
            alloc.NameRelations(set);
            alloc.NameVariables(set);
            return set;
        }

        public static void DoSelect(PathTree<BasiqueColumn> to, IList<PathTree<BasiqueColumn>> contexts, IList<ParameterExpression> parameters, Expression expr)
        {
            foreach (var (path, setter) in InitList.GetInitList(parameters, expr).WalkValues())
            {
                if (setter is ContextPredicate ctx)
                {
                    to.Set(path, contexts[ctx.ContextId]);
                }
                else if (setter is SubPredicate sub)
                {
                    if (sub.From is not ContextPredicate subctx) throw new NotImplementedException();

                    to.Set(path, contexts[subctx.ContextId].GetByPath(sub.Path));
                }
            }
        }
    }
    public class SqlUpdateData : SqlSelectorData
    {
        public UpdateContext UpdateContext;
    }

    public class SqlSelectorData
    {
        public int? Limit;
        public BasiqueTransaction Transaction;
        public Type RequestedType;
        public List<OrderByKey> OrderBy = new();
        public FlatPredicateNode Where;
        public List<JoinClause> Joins = new();
        public IRelation Relation;
        public QueryContext Context;
        public PathTree<BasiqueColumn> Columns;
    }

    public struct JoinClause
    {
        public QueryRelation Right;
        public FlatPredicateNode On;
    }

    public struct OrderByKey
    {
        public FlatPredicateNode Key;
        public bool Descending;
    }
}