using Basique.Modeling;

namespace Basique.Solve
{
    public class QueryRelation
    {
        public QueryRelation(IRelationLike relation)
        {
            Relation = relation;
        }

        public IRelationLike Relation { get; }

        public string RemoteName => Relation.Name;

        public string NamedAs { get; set; }
    }
}