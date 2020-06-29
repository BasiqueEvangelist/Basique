using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Basique.Modeling
{
    public abstract class Database
    {
        public DbConnection Connection { get; }
        internal Dictionary<Type, TableData> Tables = new Dictionary<Type, TableData>();
        public Database(DbConnection conn)
        {
            Connection = conn;
        }

        protected void Table<T>(Action<TableBuilder<T>> action)
        {
            TableData d = new TableData();
            action(new TableBuilder<T>(d));
            Tables.Add(typeof(T), d);
        }
    }
}