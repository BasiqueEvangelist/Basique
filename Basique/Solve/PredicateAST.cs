using System.Reflection;
using System;
namespace Basique.Solve
{
    public abstract class FlatPredicateNode
    {

    }
    public enum BinaryPredicateType
    {
        Equal, NotEqual, Less, Greater, AndAlso, OrElse, ExclusiveOr
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
        public override string ToString()
            => $"Context";
    }

    public class SubPredicate : FlatPredicateNode
    {
        public FlatPredicateNode From;
        public FieldInfo Field;

        public override string ToString()
            => $"Sub ({From}) ({Field.Name})";
    }
    public class ConstantPredicate : FlatPredicateNode
    {
        public object Data;
        public Type Of;
        public override string ToString()
            => $"Constant {Data}";
    }
}