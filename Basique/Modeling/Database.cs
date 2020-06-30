using System.Security.AccessControl;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Basique.Solve;

namespace Basique.Modeling
{
    public abstract class Database
    {
        public DbConnection Connection { get; }
        public IBasiqueLogger Logger { get; set; } = new EmptyLogger();
        internal Dictionary<Type, TableData> Tables = new Dictionary<Type, TableData>();
        public Database(DbConnection conn)
        {
            Connection = conn;
        }

        public async Task<BasiqueTransaction> BeginTransaction(CancellationToken token = default)
            => new BasiqueTransaction(await Connection.BeginTransactionAsync(token));

        protected void Table<T>(Action<TableBuilder<T>> action)
        {
            TableData d = new TableData();
            action(new TableBuilder<T>(d));
            Tables.Add(typeof(T), d);
        }
    }
}