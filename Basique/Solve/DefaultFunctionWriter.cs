using System;
using System.Data.Common;
using System.Text;
using Basique.Flattening;
using Basique.Modeling;

namespace Basique.Solve
{
    public class DefaultFunctionWriter : IMethodWriter
    {
        public static DefaultFunctionWriter Instance = new();

        public static bool CanProvide(CallPredicate call)
        {
            if (call.Method == KnownMethods.StartsWith)
                return true;
            else if (call.Method == KnownMethods.ReplaceChar)
                return true;
            else if (call.Method == KnownMethods.ReplaceString)
                return true;
            return false;
        }

        public int WriteMethod(CallPredicate call, IRelation mainTable, PathTreeElement<BasiqueColumn> tree, DbCommand cmd, int prefix, StringBuilder into)
        {
            if (call.Method == KnownMethods.StartsWith)
            {
                into.Append("(");
                prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Instance, cmd, prefix, into);
                into.Append(") like ");
                if (call.Arguments[0] is ConstantPredicate constant)
                {
                    into.Append("\"");
                    into.Append((string)constant.Data);
                    into.Append("%\"");
                }
                else
                {
                    switch (mainTable.Schema.SqlGeneration.Concat)
                    {
                        case SqlGenerationSettings.ConcatFunction.ConcatFunc:
                            into.Append("concat(");
                            prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Arguments[0], cmd, prefix, into);
                            into.Append(", \"%\")");
                            break;
                        case SqlGenerationSettings.ConcatFunction.DoublePipe:
                            into.Append("(");
                            prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Arguments[0], cmd, prefix, into);
                            into.Append(") || \"%\"");
                            break;
                        default: throw new NotImplementedException();
                    }
                }
            }
            else if (call.Method == KnownMethods.ReplaceString || call.Method == KnownMethods.ReplaceChar)
            {
                into.Append("replace(");
                prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Instance, cmd, prefix, into);
                into.Append(", ");
                prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Arguments[0], cmd, prefix, into);
                into.Append(", ");
                prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Arguments[1], cmd, prefix, into);
                into.Append(")");
            }
            else if (call.Method == KnownMethods.SqlLike)
            {
                into.Append("(");
                prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Arguments[0], cmd, prefix, into);
                into.Append(") like (");
                prefix = SqlBuilder.WriteSqlPredicate(mainTable, tree, call.Arguments[1], cmd, prefix, into);
                into.Append(")");
            }
            else throw new NotImplementedException();

            return prefix;
        }
    }
}