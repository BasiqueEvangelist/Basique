using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Basique.Modeling;
using Basique.Services;

namespace Basique.Solve
{
    public class ColumnSet : IDictionary<MemberInfo, BasiqueField>
    {
        readonly Dictionary<MemberInfo, BasiqueField> columns = new();

        public BasiqueField this[MemberInfo key] { get => columns[key]; set => columns[key] = value; }

        public ICollection<MemberInfo> Keys => ((IDictionary<MemberInfo, BasiqueField>)columns).Keys;

        public ICollection<BasiqueField> Values => ((IDictionary<MemberInfo, BasiqueField>)columns).Values;

        public int Count => ((ICollection<KeyValuePair<MemberInfo, BasiqueField>>)columns).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<MemberInfo, BasiqueField>>)columns).IsReadOnly;

        public void Add(MemberInfo key, BasiqueField value) => columns.Add(key, value);

        public void Add(KeyValuePair<MemberInfo, BasiqueField> item) => columns.Add(item.Key, item.Value);

        public void Clear() => columns.Clear();

        public bool Contains(KeyValuePair<MemberInfo, BasiqueField> item) => ((ICollection<KeyValuePair<MemberInfo, BasiqueField>>)columns).Contains(item);

        public bool ContainsKey(MemberInfo key) => columns.ContainsKey(key);

        public void CopyTo(KeyValuePair<MemberInfo, BasiqueField>[] array, int arrayIndex) => ((ICollection<KeyValuePair<MemberInfo, BasiqueField>>)columns).CopyTo(array, arrayIndex);

        public BasiqueField GetByPath(MemberPath path)
        {
            var field = new BasiqueField(this);
            foreach (var member in path.Members)
                field = field.AssertComposite()[member];
            return field;
        }

        public BasiqueField GetByPathCreating(MemberPath path)
        {
            var field = new BasiqueField(this);
            for (int i = 0; i < path.Members.Length; i++)
            {
                var set = field.AssertComposite();
                if (!set.columns.TryGetValue(path.Members[i], out var newField))
                {
                    newField = new BasiqueField(new ColumnSet());
                    set[path.Members[i]] = newField;
                }
                field = newField;
            }
            return field;
        }

        public IEnumerator<KeyValuePair<MemberInfo, BasiqueField>> GetEnumerator() => columns.GetEnumerator();

        public bool Remove(MemberInfo key) => columns.Remove(key);

        public bool Remove(KeyValuePair<MemberInfo, BasiqueField> item) => ((ICollection<KeyValuePair<MemberInfo, BasiqueField>>)columns).Remove(item);

        public void Set(MemberPath path, BasiqueColumn column)
        {
            ColumnSet set = GetByPathCreating(path.LastAccessed()).AssertComposite();
            set[path.Members[^1]] = new BasiqueField(column);
        }

        public bool TryGetValue(MemberInfo key, out BasiqueField value) => columns.TryGetValue(key, out value);

        public IEnumerable<KeyValuePair<MemberPath, BasiqueColumn>> WalkColumns()
        {
            foreach (var pair in columns)
            {
                if (pair.Value.HasComposite)
                {
                    foreach (var value in pair.Value.AssertComposite().WalkColumns())
                    {
                        yield return KeyValuePair.Create(value.Key.Prepend(pair.Key), value.Value);
                    }
                }
                else
                {
                    yield return KeyValuePair.Create(new MemberPath(pair.Key), pair.Value.AssertColumn());
                }
            }
        }

        public IEnumerable<ColumnSet> WalkComposites()
        {
            yield return this;
            foreach (var pair in columns)
            {
                if (pair.Value.HasComposite)
                {
                    foreach (var value in pair.Value.AssertComposite().WalkComposites())
                    {
                        yield return value;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dump(IBasiqueLogger logger)
        {
            foreach (var (path, col) in WalkColumns())
            {
                logger.Log(LogLevel.Trace, $"{path}: {col.NamedAs}");
            }
        }
    }

    public struct BasiqueField
    {
        private readonly ColumnSet composite;
        private readonly BasiqueColumn column;

        public BasiqueField(ColumnSet composite)
        {
            this.composite = composite;
            column = null;
        }

        public BasiqueField(BasiqueColumn column)
        {
            this.column = column;
            composite = null;
        }

        public bool HasComposite => composite != null;
        public bool HasColumn => column != null;

        public ColumnSet AssertComposite()
        {
            if (!HasComposite) throw new InvalidOperationException();
            return composite;
        }

        public BasiqueColumn AssertColumn()
        {
            if (!HasColumn) throw new InvalidOperationException();
            return column;
        }
    }

    public class BasiqueColumn
    {
        public IQueryRelation From;
        public ColumnData Column;

        public string NamedAs { get; set; }
    }
}