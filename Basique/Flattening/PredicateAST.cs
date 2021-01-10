using System.Reflection;
using System;
using Basique.Services;

namespace Basique.Flattening
{
    public abstract class FlatPredicateNode
    {

    }
    public enum BinaryPredicateType
    {
        Equal, NotEqual, Less, Greater, AndAlso, OrElse, ExclusiveOr, LessOrEqual, GreaterOrEqual, Add, Subtract, Multiply, Divide, Modulo
    }
    public class BinaryPredicate : FlatPredicateNode
    {
        public FlatPredicateNode Left;
        public FlatPredicateNode Right;
        public BinaryPredicateType Type;

        public override string ToString()
            => $"{Enum.GetName(typeof(BinaryPredicateType), Type)} ({Left}), ({Right})";
    }
    public enum UnaryPredicateType
    {
        Not
    }
    public class UnaryPredicate : FlatPredicateNode
    {
        public FlatPredicateNode Operand;
        public UnaryPredicateType Type;

        public override string ToString()
            => $"{Enum.GetName(typeof(UnaryPredicateType), Type)} ({Operand})";
    }

    public class TernaryPredicate : FlatPredicateNode
    {
        public FlatPredicateNode Condition;
        public FlatPredicateNode OnTrue;
        public FlatPredicateNode OnFalse;

        public override string ToString()
            => $"Ternary ({Condition}) ({OnTrue}) ({OnFalse})";
    }

    public class ContextPredicate : FlatPredicateNode
    {
        public Type Of;
        public int ContextId;
        public override string ToString()
            => $"Context {ContextId}";
    }

    public class SubPredicate : FlatPredicateNode
    {
        public FlatPredicateNode From;
        public MemberPath Path;

        public override string ToString()
            => $"Sub ({From}) ({Path})";
    }
    public class ConstantPredicate : FlatPredicateNode
    {
        public object Data;
        public Type Of;
        public override string ToString()
            => $"Constant {Data}";
    }
}