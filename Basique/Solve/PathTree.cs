using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Basique.Modeling;
using Basique.Services;

namespace Basique.Solve
{
    public class PathTree<T> : IDictionary<MemberInfo, PathTreeElement<T>>
    {
        readonly Dictionary<MemberInfo, PathTreeElement<T>> columns = new();

        public PathTreeElement<T> this[MemberInfo key] { get => columns[key]; set => columns[key] = value; }

        public ICollection<MemberInfo> Keys => columns.Keys;

        public ICollection<PathTreeElement<T>> Values => columns.Values;

        public int Count => columns.Count;

        public bool IsReadOnly => false;

        public void Add(MemberInfo key, PathTreeElement<T> value) => columns.Add(key, value);

        public void Add(KeyValuePair<MemberInfo, PathTreeElement<T>> item) => columns.Add(item.Key, item.Value);

        public void Clear() => columns.Clear();

        public bool Contains(KeyValuePair<MemberInfo, PathTreeElement<T>> item) => ((ICollection<KeyValuePair<MemberInfo, PathTreeElement<T>>>)columns).Contains(item);

        public bool ContainsKey(MemberInfo key) => columns.ContainsKey(key);

        public void CopyTo(KeyValuePair<MemberInfo, PathTreeElement<T>>[] array, int arrayIndex) => ((ICollection<KeyValuePair<MemberInfo, PathTreeElement<T>>>)columns).CopyTo(array, arrayIndex);

        public PathTreeElement<T> GetByPath(MemberPath path)
        {
            var field = new PathTreeElement<T>(this);
            foreach (var member in path.Members)
                field = field.Tree[member];
            return field;
        }

        public PathTreeElement<T> GetByPathCreating(MemberPath path)
        {
            var field = new PathTreeElement<T>(this);
            for (int i = 0; i < path.Members.Length; i++)
            {
                var set = field.Tree;
                if (!set.columns.TryGetValue(path.Members[i], out var newField))
                {
                    newField = new PathTreeElement<T>(new PathTree<T>());
                    set[path.Members[i]] = newField;
                }
                field = newField;
            }
            return field;
        }

        public IEnumerator<KeyValuePair<MemberInfo, PathTreeElement<T>>> GetEnumerator() => columns.GetEnumerator();

        public bool Remove(MemberInfo key) => columns.Remove(key);

        public bool Remove(KeyValuePair<MemberInfo, PathTreeElement<T>> item) => ((ICollection<KeyValuePair<MemberInfo, PathTreeElement<T>>>)columns).Remove(item);

        public void Set(MemberPath path, T value) => Set(path, new PathTreeElement<T>(value));

        public void Set(MemberPath path, PathTree<T> composite) => Set(path, new PathTreeElement<T>(composite));

        public void Set(MemberPath path, PathTreeElement<T> field)
        {
            PathTree<T> set = GetByPathCreating(path.LastAccessed()).Tree;
            set[path.Members[^1]] = field;
        }

        public bool TryGetValue(MemberInfo key, out PathTreeElement<T> value) => columns.TryGetValue(key, out value);

        public IEnumerable<KeyValuePair<MemberPath, T>> WalkValues()
        {
            foreach (var pair in columns)
            {
                if (pair.Value.IsTree)
                {
                    foreach (var value in pair.Value.Tree.WalkValues())
                    {
                        yield return KeyValuePair.Create(value.Key.Prepend(pair.Key), value.Value);
                    }
                }
                else
                {
                    yield return KeyValuePair.Create(new MemberPath(pair.Key), pair.Value.Value);
                }
            }
        }

        public IEnumerable<PathTree<T>> WalkTrees()
        {
            yield return this;
            foreach (var pair in columns)
            {
                if (pair.Value.IsTree)
                {
                    foreach (var value in pair.Value.Tree.WalkTrees())
                    {
                        yield return value;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public struct PathTreeElement<T>
    {
        private readonly PathTree<T> tree;
        private readonly T value;

        public PathTreeElement(PathTree<T> subtree)
        {
            tree = subtree;
            value = default;
        }

        public PathTreeElement(T value)
        {
            tree = null;
            this.value = value;
        }

        public bool IsTree => tree != null;

        public PathTree<T> Tree
        {
            get
            {
                if (!IsTree) throw new InvalidOperationException();
                return tree;
            }
        }

        public T Value
        {
            get
            {
                if (IsTree) throw new InvalidOperationException();
                return value;
            }
        }
    }

    public class BasiqueColumn
    {
        public QueryRelation From;
        public ColumnData Column;

        public string NamedAs { get; set; }
    }
}