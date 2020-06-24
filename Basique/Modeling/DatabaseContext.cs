using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Basique.Modeling
{
    public abstract class DatabaseContext
    {
        public DbConnection Connection { get; }
        internal Dictionary<Type, TableData> tables = new Dictionary<Type, TableData>();
        public DatabaseContext(DbConnection conn)
        {
            Connection = conn;
        }

        protected void Table<T>(Action<TableBuilder<T>> action)
        {
            TableData d = new TableData();
            action(new TableBuilder<T>(d));
            tables.Add(typeof(T), d);
        }
    }
}