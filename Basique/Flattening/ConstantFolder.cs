using System.Linq;

namespace Basique.Flattening
{
    public class ConstantFolder : PredicateTreeTransformer
    {
        public static readonly ConstantFolder Instance = new();

        protected override FlatPredicateNode TransformSubPredicate(SubPredicate node)
        {
            base.TransformSubPredicate(node);

            if (node.From is ConstantPredicate or null)
                return new ConstantPredicate(node.Path.Follow((node.From as ConstantPredicate)?.Data));

            return node;
        }
        protected override FlatPredicateNode TransformCallPredicate(CallPredicate node)
        {
            base.TransformCallPredicate(node);

            if (node.Instance is ConstantPredicate or null && node.Arguments.All(x => x is ConstantPredicate))
                return new ConstantPredicate(node.Method.Invoke(
                    (node.Instance as ConstantPredicate)?.Data,
                    node.Arguments.Cast<ConstantPredicate>().Select(x => x.Data).ToArray()
                ));

            return node;
        }
    }
}