using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System;
using Basique.Solve;
using System.Linq;

namespace Basique.Modeling
{
    public class TableData
    {
        public string Name;
        public Dictionary<string, ColumnData> Columns = new Dictionary<string, ColumnData>();
    }
    public class ColumnData
    {
        public string Name;
        public TableData Table;
        public MemberPath Path;
        public Type Type;
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
            var columnData = new ColumnData();
            columnData.Table = data;
            var path = MemberPath.Create(selector.Body);
            columnData.Path = path.Path;
            columnData.Name = path.Path.Members[^1].Name.ToLower();

            var lastMember = path.Path.Members[^1];
            if (lastMember is FieldInfo field)
                columnData.Type = field.FieldType;
            else if (lastMember is PropertyInfo prop)
                columnData.Type = prop.PropertyType;

            data.Columns.Add(path.Path.ToString(), columnData);
            return new ColumnBuilder<TField>(columnData);
        }
    }
    public class ColumnBuilder<T>
    {
        private ColumnData data;

        internal ColumnBuilder(ColumnData data)
        {
            this.data = data;
        }

        public ColumnBuilder<T> RemoteName(string name)
        {
            data.Name = name;
            return this;
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