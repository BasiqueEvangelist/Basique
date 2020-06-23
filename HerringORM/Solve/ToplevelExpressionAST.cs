using System;
using System.Linq.Expressions;
using HerringORM.Modeling;

namespace HerringORM.Solve
{
    public abstract class ExpressionNode
    {
        public ExpressionNode Parent;
        public abstract void Dump();
    }

    public class FinalExpressionNode : ExpressionNode
    {
        public ITable Table { get; }

        public FinalExpressionNode(ITable tab)
        {
            Table = tab;
            Parent = this;
        }

        public override void Dump()
        {
            Console.WriteLine("Final");
        }
    }

    public class WhereExpressionNode : ExpressionNode
    {
        public Expression Condition;
        public override void Dump()
        {
            Parent.Dump();
            Console.WriteLine("Where ({0})", Condition);
        }
    }

    public class PullExpressionNode : ExpressionNode
    {
        public enum PullType
        {
            List, Array
        }

        public PullType Type;

        public override void Dump()
        {
            Parent.Dump();
            Console.WriteLine("Pull ({0})", Enum.GetName(typeof(PullType), Type));
        }
    }

    public class CreateExpressionNode : ExpressionNode
    {
        public Type OfType;
        public MemberInitExpression Factory;

        public override void Dump()
        {
            Parent.Dump();
            Console.WriteLine("Create {0} ({1})", OfType, Factory);
        }
    }
}