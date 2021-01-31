using System.Reflection;
using System;
using Basique.Services;
using Basique.Solve;
using System.Linq;

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

        public static UnaryPredicate Not(FlatPredicateNode operand)
        {
            return new UnaryPredicate() { Operand = operand, Type = UnaryPredicateType.Not };
        }
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
        public ConstantPredicate() { }
        public ConstantPredicate(object obj)
        {
            Data = obj;
            Of = obj?.GetType() ?? typeof(object);
        }

        public object Data;
        public Type Of;
        public override string ToString()
            => $"Constant {Data}";
    }
    public class ColumnPredicate : FlatPredicateNode
    {
        public BasiqueColumn Column;

        public override string ToString()
            => $"Column {Column.From.RemoteName}.{Column.Column.Name}";
    }
    public class CallPredicate : FlatPredicateNode
    {
        public FlatPredicateNode Instance;
        public MethodInfo Method;
        public FlatPredicateNode[] Arguments;

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(x => x.ToString()));
            return $"Column {Instance}.{Method}({args})";
        }
    }
}