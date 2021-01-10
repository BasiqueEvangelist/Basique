using System;
using System.Reflection;
using Basique.Flattening;
using Basique.Solve;

namespace Basique.Modeling
{
    public interface IJoinRelation : IRelation
    {
        public IJoinSideRelation Left { get; }
        public IJoinSideRelation Right { get; }
        public FlatPredicateNode On { get; }
    }

    public class Join<TLeft, TRight> : RelationBase<(TLeft, TRight)>, IJoinRelation
    {
        private readonly static FieldInfo item1Field = typeof(ValueTuple<,>).GetField("Item1");
        private readonly static FieldInfo item2Field = typeof(ValueTuple<,>).GetField("Item2");

        public JoinSide<TLeft> Left
        { get; }
        public JoinSide<TRight> Right { get; }
        public FlatPredicateNode On { get; }
        public override string Name => throw new InvalidOperationException();

        IJoinSideRelation IJoinRelation.Left => Left;
        IJoinSideRelation IJoinRelation.Right => Right;


        public Join(BasiqueSchema conn, RelationBase<TLeft> left, RelationBase<TRight> right, FlatPredicateNode on) : base(conn)
        {
            Left = new JoinSide<TLeft>(this, left);
            Right = new JoinSide<TRight>(this, right);
            On = on;
        }

        public override void FillSet(ColumnSet set, QueryContext ctx)
        {
            ColumnSet leftSet = new(), rightSet = new();

            Left.FillSet(leftSet, ctx);
            Right.FillSet(rightSet, ctx);

            var tupleType = typeof(ValueTuple<,>).MakeGenericType(typeof(TLeft), typeof(TRight));

            set[tupleType.GetField("Item1")] = new BasiqueField(leftSet);
            set[tupleType.GetField("Item2")] = new BasiqueField(rightSet);
        }
        public override IQueryRelation MintLogical() => throw new InvalidOperationException();
    }

    public interface IJoinSideRelation : IRelationLike
    {
        public IJoinRelation Join { get; }
        public IRelation Original { get; }
    }

    public class JoinSide<T> : IJoinSideRelation
    {
        public IJoinRelation Join { get; }

        public JoinSide(IJoinRelation join, RelationBase<T> original)
        {
            Join = join;
            Original = original;
        }

        public RelationBase<T> Original { get; }
        IRelation IJoinSideRelation.Original => Original;

        public string Name => Original.Name;

        public void FillSet(ColumnSet set, QueryContext ctx)
        {
            foreach (var (path, column) in Join.Schema.Tables[Original.ElementType].Columns)
            {
                set.Set(path, new BasiqueColumn()
                {
                    From = ctx.GetLogical(this),
                    Column = column
                });
            }
        }
        public IQueryRelation MintLogical() => new JoinSideQueryRelation(this);
    }
}