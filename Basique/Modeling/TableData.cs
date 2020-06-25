using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System;

namespace Basique.Modeling
{
    internal class TableData
    {
        public string Name;
        public Dictionary<MemberInfo, ColumnData> Columns = new Dictionary<MemberInfo, ColumnData>();
    }
    internal class ColumnData
    {
        public string Name;
        public Type Of;
        public bool IsId;
        public bool Generated;
    }
    public class TableBuilder<T>
    {
        private readonly TableData data;

        internal TableBuilder(TableData data)
        {
            this.data = data;
        }

        public void RemoteName(string name) => data.Name = name;
        public ColumnBuilder<TField> Field<TField>(Expression<Func<T, TField>> selector)
        {
            ColumnData cd = new ColumnData();
            MemberInfo member = (selector.Body as MemberExpression).Member;
            cd.Name = member.Name.ToLower();
            if (member is FieldInfo field)
                cd.Of = field.FieldType;
            else if (member is PropertyInfo prop)
                cd.Of = prop.PropertyType;
            data.Columns.Add(member, cd);
            return new ColumnBuilder<TField>(cd);
        }
    }
    public class ColumnBuilder<T>
    {
        private ColumnData data;

        internal ColumnBuilder(ColumnData data)
        {
            this.data = data;
        }

        public ColumnBuilder<T> Id()
        {
            data.IsId = true;
            return this;
        }

        public ColumnBuilder<T> Generated()
        {
            data.Generated = true;
            return this;
        }
    }
}