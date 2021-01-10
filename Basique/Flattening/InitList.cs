using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Basique.Services;

namespace Basique.Flattening
{
    public static class InitList
    {
        public static IEnumerable<KeyValuePair<MemberPath, FlatPredicateNode>> GetInitList(IList<ParameterExpression> parameters, Expression expr)
        {
            if (expr is MemberInitExpression memberInit)
            {
                foreach (var assign in memberInit.Bindings.OfType<MemberAssignment>())
                {
                    yield return KeyValuePair.Create(new MemberPath(assign.Member), PredicateFlattener.Flatten(assign.Expression, parameters));
                }
            }
            else if (expr is NewExpression newExpr)
            {
                // Assume this is an anonymous type and hope for the best.

                foreach (var (member, param) in newExpr.Members.Zip(newExpr.Arguments, (x, y) => (x, y)))
                {
                    yield return KeyValuePair.Create(new MemberPath(member), PredicateFlattener.Flatten(param, parameters));
                }
            }
            else throw new NotImplementedException();
        }
    }
}