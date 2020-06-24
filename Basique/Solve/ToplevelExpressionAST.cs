using System;
using System.Linq.Expressions;
using Basique.Modeling;
using NLog;

namespace Basique.Solve
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
        public FlatPredicateNode Condition;
        public override void Dump()
        {
            Parent.Dump();
            LOGGER.Trace("Where ({0})", Condition);
        }
    }

    public class LimitExpressionNode : ExpressionNode
    {
        public int Count;
        public override void Dump()
        {
            Parent.Dump();
            LOGGER.Trace("Take ({0})", Count);
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

    public class UpdateExpressionNode : ExpressionNode
    {
        public UpdateContext Context;

        public override void Dump()
        {
            Parent.Dump();
            LOGGER.Trace("Update");
        }
    }
}