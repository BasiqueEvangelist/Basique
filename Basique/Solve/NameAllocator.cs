using System;
using System.Collections.Generic;
using Basique.Services;

namespace Basique.Solve
{
    public class NameAllocator
    {
        private readonly IBasiqueLogger logger;
        private readonly Dictionary<string, QueryRelation> relations = new();
        private readonly Dictionary<string, BasiqueColumn> columns = new();
        public NameAllocator(IBasiqueLogger logger)
        {
            this.logger = logger;
        }

        public void NameRelations(PathTree<BasiqueColumn> set)
        {
            foreach (var pair in set.WalkValues())
            {
                var relation = pair.Value.From;
                var suffix = 0;

                if (relation.NamedAs == null)
                {
                    relation.NamedAs = relation.RemoteName;
                    while (relations.ContainsKey(relation.NamedAs))
                    {
                        relation.NamedAs = relation.RemoteName + "_" + suffix++;
                    }

                    logger.Log(LogLevel.Trace, $"Named {relation.RemoteName} as {relation.NamedAs}");

                    relations[relation.NamedAs] = relation;
                }
            }
        }

        public void NameVariables(PathTree<BasiqueColumn> set)
        {
            foreach (var composite in set.WalkTrees())
            {
                foreach (var (_, field) in composite)
                {
                    if (!field.IsTree)
                    {
                        var column = field.Value;
                        var suffix = 0;

                        if (column.NamedAs == null)
                        {
                            column.NamedAs = column.Column.Name;
                            while (columns.ContainsKey(column.NamedAs))
                            {
                                column.NamedAs = column.Column.Name + "_" + suffix++;
                            }

                            logger.Log(LogLevel.Trace, $"Named {column.Column.Name} as {column.NamedAs}");

                            columns[column.NamedAs] = column;
                        }
                    }
                }
            }
        }
    }
}