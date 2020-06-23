using System.Reflection;
using System;
namespace HerringORM.Solve
{
    public abstract class FlatPredicateNode
    {

    }
    public class EqualPredicate : FlatPredicateNode
    {
        public FlatPredicateNode Left;
        public FlatPredicateNode Right;

        public override string ToString()
            => $"Equal ({Left}), ({Right})";
    }

    public class VariablePredicate : FlatPredicateNode
    {
        public Type Of;
    }

    public class SubPredicate : FlatPredicateNode
    {
        public FlatPredicateNode From;
        public FieldInfo Field;
    }
    public class ConstantPredicate : FlatPredicateNode
    {
        public object Data;
        public Type Of;
    }
}