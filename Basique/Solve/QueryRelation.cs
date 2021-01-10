using Basique.Modeling;

namespace Basique.Solve
{
    public interface IQueryRelation
    {
        public string RemoteName { get; }
        public string NamedAs { get; set; }
    }

    public class DirectQueryRelation : IQueryRelation
    {
        public DirectQueryRelation(IRelation relation)
        {
            Relation = relation;
        }

        public IRelation Relation { get; }

        public string RemoteName => Relation.Name;

        public string NamedAs { get; set; }
    }
}