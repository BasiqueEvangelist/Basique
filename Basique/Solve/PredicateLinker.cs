using System.Collections.Generic;
using Basique.Flattening;

namespace Basique.Solve
{
    public class PredicateLinker : PredicateTreeTransformer
    {
        public IList<PathTreeElement<BasiqueColumn>> Contexts { get; set; }

        public PredicateLinker(IList<PathTreeElement<BasiqueColumn>> ctxs)
        {
            Contexts = ctxs;
        }

        protected override FlatPredicateNode TransformSubPredicate(SubPredicate node)
        {
            base.TransformSubPredicate(node);
            if (node.From is ContextPredicate ctx)
                return new ColumnPredicate() { Column = Contexts[ctx.ContextId].GetByPath(node.Path).Value };

            return node;
        }
    }
}