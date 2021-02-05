using System.Linq;
using System.Reflection;

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

            if (node.Instance is ConstantPredicate or null && node.Arguments.All(x => x is ConstantPredicate) && ShouldFoldMethod(node.Method))
                return new ConstantPredicate(node.Method.Invoke(
                    (node.Instance as ConstantPredicate)?.Data,
                    node.Arguments.Cast<ConstantPredicate>().Select(x => x.Data).ToArray()
                ));

            return node;
        }

        public static bool ShouldFoldMethod(MethodInfo info)
        {
            if (info.GetCustomAttribute<NoConstantFoldingAttribute>() != null)
                return false;
            else if (info.GetCustomAttribute<MethodWriterAttribute>() != null)
                return false;
            else
                return true;
        }
    }
}