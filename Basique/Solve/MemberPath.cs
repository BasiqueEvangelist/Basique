using System.Linq.Expressions;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Basique.Solve
{
    public struct MemberPath
    {
        public Type Start;
        public MemberInfo[] Members;

        public MemberPath(MemberInfo mem)
        {
            Start = mem.DeclaringType;
            Members = new[] { mem };
        }

        public static (MemberPath Path, Expression Final) Create(Expression expr)
        {
            MemberPath path;
            if (expr is ParameterExpression par)
            {
                path.Members = new MemberInfo[0];
                path.Start = par.Type;
                return (path, expr);
            }
            else if (expr is ConstantExpression con)
            {
                path.Members = new MemberInfo[0];
                path.Start = con.Type;
                return (path, expr);
            }
            else if (expr is MemberExpression mem)
            {
                List<MemberInfo> foundMembers = new List<MemberInfo>();
                Expression subexpr = mem;
                while (subexpr is MemberExpression submem)
                {
                    foundMembers.Add(submem.Member);
                    subexpr = submem.Expression;
                }
                if (subexpr is ParameterExpression param)
                    path.Start = param.Type;
                else if (subexpr is ConstantExpression cons)
                    path.Start = cons.Type;
                else
                    throw new NotImplementedException("Final expression must be a parameter or variable");
                foundMembers.Reverse();
                path.Members = foundMembers.ToArray();
                return (path, subexpr);
            }
            else
                throw new NotImplementedException("Final expression must be a parameter or variable");
        }

        public object Follow(object from)
        {
            foreach (var member in Members)
                if (member is FieldInfo field)
                    from = field.GetValue(from);
                else if (member is PropertyInfo prop)
                    from = prop.GetValue(from);
                else
                    throw new NotImplementedException();
            return from;
        }

        public void Set(object from, object value)
        {
            for (var i = 0; i < Members.Length - 1; i++)
            {
                object newObj;
                if (Members[i] is FieldInfo field)
                {
                    newObj = field.GetValue(from);

                    if (newObj == null)
                    {
                        newObj = Activator.CreateInstance(field.FieldType);
                        field.SetValue(from, newObj);
                    }
                }
                else if (Members[i] is PropertyInfo prop)
                {
                    newObj = prop.GetValue(from);

                    if (newObj == null)
                    {
                        newObj = Activator.CreateInstance(prop.PropertyType);
                        prop.SetValue(from, newObj);
                    }
                }
                else
                    throw new NotImplementedException();

                from = newObj;
            }

            if (Members[^1] is FieldInfo fieldLast)
                fieldLast.SetValue(from, value);
            else if (Members[^1] is PropertyInfo propLast)
                propLast.SetValue(from, value);
            else
                throw new NotImplementedException();
        }

        public MemberPath LastAccessed()
        {
            if (Members.Length < 1)
                throw new InvalidOperationException();

            return new MemberPath() { Start = Start, Members = Members.SkipLast(1).ToArray() };
        }

        public override string ToString()
        {
            return $"({Start.FullName}) {string.Join(" -> ", Members.Select(x => x.Name))}";
        }

        public static bool operator ==(MemberPath a, MemberPath b)
        {
            return a.Start == b.Start &&
                   Enumerable.SequenceEqual(a.Members, b.Members);
        }

        public static bool operator !=(MemberPath a, MemberPath b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, Members);
        }

        public override bool Equals(object obj)
        {
            return obj is MemberPath path &&
                   this == path;
        }
    }
}