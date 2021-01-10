using System.Collections.Generic;
using Basique.Modeling;

namespace Basique.Solve
{
    public class QueryContext
    {
        private readonly Dictionary<IRelation, IQueryRelation> relations = new();

        public IQueryRelation GetLogical(IRelation relation)
        {
            if (!relations.TryGetValue(relation, out var logical))
            {
                logical = relation.MintLogical();
                relations.Add(relation, logical);
            }

            return logical;
        }
    }
}