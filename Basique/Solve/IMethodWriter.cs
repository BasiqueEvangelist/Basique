using System.Data.Common;
using System.Text;
using Basique.Flattening;
using Basique.Modeling;

namespace Basique.Solve
{
    public interface IMethodWriter
    {
        int WriteMethod(CallPredicate call, IRelation mainTable, PathTree<BasiqueColumn> tree, DbCommand cmd, int prefix, StringBuilder into);
    }
}