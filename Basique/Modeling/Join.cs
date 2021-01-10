using System;
using System.Linq.Expressions;
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

    public class Join<TLeft, TRight, TResult> : RelationBase<TResult>, IJoinRelation
    {
        public JoinSide<TLeft> Left { get; }
        public JoinSide<TRight> Right { get; }
        public FlatPredicateNode On { get; }
        private readonly LambdaExpression factory;
        public override string Name => throw new InvalidOperationException();

        IJoinSideRelation IJoinRelation.Left => Left;
        IJoinSideRelation IJoinRelation.Right => Right;


        public Join(BasiqueSchema conn, RelationBase<TLeft> left, RelationBase<TRight> right, FlatPredicateNode on, LambdaExpression factory) : base(conn)
        {
            Left = new JoinSide<TLeft>(this, left);
            Right = new JoinSide<TRight>(this, right);
            this.factory = factory;
            On = on;
        }

        public override void FillSet(PathTree<BasiqueColumn> set, QueryContext ctx)
        {
            PathTree<BasiqueColumn> leftSet = new(), rightSet = new();

            Left.FillSet(leftSet, ctx);
            Right.FillSet(rightSet, ctx);

            LinqVM.DoSelect(set, new[] { leftSet, rightSet }, factory.Parameters, factory.Body);
        }
        public override QueryRelation MintLogical() => throw new InvalidOperationException();
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

        public void FillSet(PathTree<BasiqueColumn> set, QueryContext ctx)
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

        public QueryRelation MintLogical() => new(this);
    }
}