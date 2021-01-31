using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Basique.Solve;

namespace Basique.Flattening
{
    public static class InitList
    {
        public static PathTreeElement<FlatPredicateNode> GetInitList(IList<ParameterExpression> parameters, Expression expr)
        {
            var tree = new PathTree<FlatPredicateNode>();

            if (expr is MemberInitExpression memberInit)
            {
                foreach (var assign in memberInit.Bindings.OfType<MemberAssignment>())
                {
                    tree[assign.Member] = new PathTreeElement<FlatPredicateNode>(PredicateFlattener.Flatten(assign.Expression, parameters));
                }
            }
            else if (expr is NewExpression newExpr)
            {
                // Assume this is an anonymous type and hope for the best.

                foreach (var (member, param) in newExpr.Members.Zip(newExpr.Arguments, (x, y) => (x, y)))
                {
                    tree[member] = new PathTreeElement<FlatPredicateNode>(PredicateFlattener.Flatten(param, parameters));
                }
            }
            else if (expr is MemberExpression member)
            {
                return new PathTreeElement<FlatPredicateNode>(PredicateFlattener.Flatten(member, parameters));
            }
            else throw new NotImplementedException();

            return new PathTreeElement<FlatPredicateNode>(tree);
        }
    }
}