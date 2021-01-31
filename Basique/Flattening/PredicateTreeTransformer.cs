using System;
using System.Collections.Generic;

namespace Basique.Flattening
{
    public abstract class PredicateTreeTransformer
    {
        public virtual FlatPredicateNode TransformNode(FlatPredicateNode node)
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
                CallPredicate call => TransformCallPredicate(call),
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
        protected virtual FlatPredicateNode TransformCallPredicate(CallPredicate node)
        {
            if (node.Instance != null)
                node.Instance = TransformNode(node.Instance);

            for (int i = 0; i < node.Arguments.Length; i++)
                node.Arguments[i] = TransformNode(node.Arguments[i]);

            return node;
        }
    }

    public class StackTransformer : PredicateTreeTransformer
    {
        private readonly IEnumerable<PredicateTreeTransformer> transformers;

        public StackTransformer(IEnumerable<PredicateTreeTransformer> transformers)
        {
            this.transformers = transformers;
        }

        public override FlatPredicateNode TransformNode(FlatPredicateNode node)
        {
            foreach (var trans in transformers)
                node = trans.TransformNode(node);
            return node;
        }
    }
}