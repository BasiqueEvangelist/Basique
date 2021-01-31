using System;

namespace Basique.Flattening
{
    public abstract class PredicateTreeTransformer
    {
        public FlatPredicateNode TransformNode(FlatPredicateNode node)
        {
            return node switch
            {
                BinaryPredicate bin => TransformBinaryPredicate(bin),
                UnaryPredicate un => TransformUnaryPredicate(un),
                TernaryPredicate ter => TransformTernaryPredicate(ter),
                ContextPredicate ctx => TransformContextPredicate(ctx),
                SubPredicate sub => TransformSubPredicate(sub),
                ConstantPredicate con => TransformConstantPredicate(con),
                ColumnPredicate col => TransformColumnPredicate(col),
                _ => throw new NotImplementedException(),
            };
        }

        protected virtual FlatPredicateNode TransformBinaryPredicate(BinaryPredicate node)
        {
            node.Left = TransformNode(node.Left);
            node.Right = TransformNode(node.Right);
            return node;
        }
        protected virtual FlatPredicateNode TransformUnaryPredicate(UnaryPredicate node)
        {
            node.Operand = TransformNode(node.Operand);
            return node;
        }
        protected virtual FlatPredicateNode TransformTernaryPredicate(TernaryPredicate node)
        {
            node.Condition = TransformNode(node.Condition);
            node.OnTrue = TransformNode(node.OnTrue);
            node.OnFalse = TransformNode(node.OnFalse);
            return node;
        }
        protected virtual FlatPredicateNode TransformContextPredicate(ContextPredicate node) => node;
        protected virtual FlatPredicateNode TransformSubPredicate(SubPredicate node)
        {
            node.From = TransformNode(node.From);
            return node;
        }
        protected virtual FlatPredicateNode TransformConstantPredicate(ConstantPredicate node) => node;
        protected virtual FlatPredicateNode TransformColumnPredicate(ColumnPredicate node) => node;
    }
}