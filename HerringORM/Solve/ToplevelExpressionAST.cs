using System;
using System.Linq.Expressions;
using HerringORM.Modeling;
using NLog;

namespace HerringORM.Solve
{
    public abstract class ExpressionNode
    {
        internal static Logger LOGGER = LogManager.GetCurrentClassLogger();
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
            LOGGER.Trace("Final");
        }
    }

    public class WhereExpressionNode : ExpressionNode
    {
        public Expression Condition;
        public override void Dump()
        {
            Parent.Dump();
            LOGGER.Trace("Where ({0})", Condition);
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
            LOGGER.Trace("Pull ({0})", Enum.GetName(typeof(PullType), Type));
        }
    }

    public class CreateExpressionNode : ExpressionNode
    {
        public Type OfType;
        public MemberInitExpression Factory;

        public override void Dump()
        {
            Parent.Dump();
            LOGGER.Trace("Create {0} ({1})", OfType, Factory);
        }
    }
}