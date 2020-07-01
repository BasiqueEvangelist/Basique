using System;
using System.Collections.Generic;
using Basique.Flattening;
using Basique.Modeling;

namespace Basique.Solve
{
    public static class LinqVM
    {
        public static SqlSelectorData BuildSelectorData(List<ExpressionNode> nodes, SqlSelectorData data)
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
                else if (node is TransactionExpressionNode trans)
                    data.Transaction = trans.Transaction;
                else if (node is WhereExpressionNode whereexpr)
                {
                    if (data.Where == null)
                        data.Where = whereexpr.Condition;
                    else
                        data.Where = new BinaryPredicate() { Left = data.Where, Right = whereexpr.Condition, Type = BinaryPredicateType.AndAlso };
                }
                else if (node is OrderByExpressionNode orderby)
                {
                    data.OrderBy.Clear();
                    data.OrderBy.Add(new OrderByKey() { Descending = orderby.Descending, Key = orderby.Key });
                }
                else if (node is ThenByExpressionNode thenby)
                    data.OrderBy.Add(new OrderByKey() { Descending = thenby.Descending, Key = thenby.Key });
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
            BuildSelectorData(nodes, data);
            data.Context = (nodes[^1] as UpdateExpressionNode).Context;
            return data;
        }
    }
    public class SqlUpdateData : SqlSelectorData
    {
        public UpdateContext Context;
    }

    public class SqlSelectorData
    {
        public int? Limit;
        public BasiqueTransaction Transaction;
        public Type RequestedType;
        public List<OrderByKey> OrderBy = new List<OrderByKey>();
        public FlatPredicateNode Where;
        public IRelation Relation;
    }

    public struct OrderByKey
    {
        public FlatPredicateNode Key;
        public bool Descending;
    }
}