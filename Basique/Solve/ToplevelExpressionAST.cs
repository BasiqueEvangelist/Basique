using System;
using System.Linq.Expressions;
using Basique.Modeling;
using NLog;

namespace Basique.Solve
{
    public abstract class ExpressionNode
    {
        public ExpressionNode Parent;
        public abstract void Dump(ILogger log);
    }

    public class FinalExpressionNode : ExpressionNode
    {
        public IRelation Table { get; }

        public FinalExpressionNode(IRelation tab)
        {
            Table = tab;
            Parent = this;
        }

        public override void Dump(ILogger log)
        {
            log.Trace("Final");
        }
    }

    public class WhereExpressionNode : ExpressionNode
    {
        public FlatPredicateNode Condition;
        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("Where ({0})", Condition);
        }
    }

    public class OrderByExpressionNode : ExpressionNode
    {
        public bool Descending;
        public FlatPredicateNode Key;
        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("OrderBy{0} ({1})", Descending ? "Descending" : "", Key);
        }
    }

    public class LimitExpressionNode : ExpressionNode
    {
        public int Count;
        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("Take ({0})", Count);
        }
    }

    public class PullExpressionNode : ExpressionNode
    {
        public enum PullType
        {
            List, Array
        }

        public PullType Type;

        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("Pull ({0})", Enum.GetName(typeof(PullType), Type));
        }
    }

    public class PullSingleExpressionNode : ExpressionNode
    {
        public enum PullType
        {
            First, Single
        }

        public PullType Type;
        public bool IncludeDefault;
        public FlatPredicateNode By;

        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("PullSingle ({0})", Enum.GetName(typeof(PullType), Type));
        }
    }

    public class CreateExpressionNode : ExpressionNode
    {
        public Type OfType;
        public MemberInitExpression Factory;

        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("Create {0} ({1})", OfType, Factory);
        }
    }

    public class UpdateExpressionNode : ExpressionNode
    {
        public UpdateContext Context;

        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("Update");
        }
    }

    public class DeleteExpressionNode : ExpressionNode
    {
        public override void Dump(ILogger log)
        {
            Parent.Dump(log);
            log.Trace("Delete");
        }
    }
}