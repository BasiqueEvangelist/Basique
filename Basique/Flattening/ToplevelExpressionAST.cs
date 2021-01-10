using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Basique.Modeling;
using Basique.Services;

namespace Basique.Flattening
{
    public abstract class ExpressionNode
    {
        public ExpressionNode Parent;
        public abstract void Dump(IBasiqueLogger log);
    }

    public class FinalExpressionNode : ExpressionNode
    {
        public IRelation Table { get; }

        public FinalExpressionNode(IRelation tab)
        {
            Table = tab;
            Parent = this;
        }

        public override void Dump(IBasiqueLogger log)
        {
            log.Log(LogLevel.Trace, "Final");
        }
    }

    public class WhereExpressionNode : ExpressionNode
    {
        public FlatPredicateNode Condition;
        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"Where ({Condition})");
        }
    }

    public class SelectExpressionNode : ExpressionNode
    {
        public LambdaExpression Via;
        public Type To;
        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"Select ({Via}) -> {To}");
        }
    }

    public class OrderByExpressionNode : ExpressionNode
    {
        public bool Descending;
        public FlatPredicateNode Key;
        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"OrderBy{(Descending ? "Descending" : "")} ({Key})");
        }
    }

    public class ThenByExpressionNode : ExpressionNode
    {
        public bool Descending;
        public FlatPredicateNode Key;
        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"ThenBy{(Descending ? "Descending" : "")} ({Key})");
        }
    }

    public class LimitExpressionNode : ExpressionNode
    {
        public int Count;
        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"Take ({Count})");
        }
    }

    public class PullExpressionNode : ExpressionNode
    {
        public enum PullType
        {
            List, Array
        }

        public PullType Type;

        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"Pull ({Enum.GetName(typeof(PullType), Type)})");
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

        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"PullSingle ({Enum.GetName(typeof(PullType), Type)})");
        }
    }

    public class CreateExpressionNode : ExpressionNode
    {
        public Type OfType;
        public Dictionary<MemberPath, FlatPredicateNode> InitList;

        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, $"Create {OfType} ({InitList})");
        }
    }

    public class UpdateExpressionNode : ExpressionNode
    {
        public UpdateContext Context;

        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, "Update");
        }
    }

    public class DeleteExpressionNode : ExpressionNode
    {
        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, "Delete");
        }
    }

    public class TransactionExpressionNode : ExpressionNode
    {
        public BasiqueTransaction Transaction;

        public override void Dump(IBasiqueLogger log)
        {
            Parent.Dump(log);
            log.Log(LogLevel.Trace, "Transaction");
        }
    }
}