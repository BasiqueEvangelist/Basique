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

    public class JoinSideQueryRelation : IQueryRelation
    {
        private readonly IJoinSideRelation side;

        public JoinSideQueryRelation(IJoinSideRelation side)
        {
            this.side = side;
        }

        public string RemoteName => side.Name;

        public string NamedAs { get; set; }
    }
}