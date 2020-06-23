using System.Reflection;
using System;
namespace HerringORM.Solve
{
    public abstract class FlatPredicateNode
    {

    }
    public enum BinaryPredicateType
    {
        Equal, NotEqual, Less, Greater, AndAlso
    }
    public class BinaryPredicate : FlatPredicateNode
    {
        public FlatPredicateNode Left;
        public FlatPredicateNode Right;
        public BinaryPredicateType Type;

        public override string ToString()
            => $"{Enum.GetName(typeof(BinaryPredicateType), Type)} ({Left}), ({Right})";
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