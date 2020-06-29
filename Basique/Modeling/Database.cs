using System.Security.AccessControl;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Basique.Solve;
using NLog;
using NLog.Config;
using Perfusion;

namespace Basique.Modeling
{
    public abstract class Database
    {
        public Container Container { get; } = new Container();
        public DbConnection Connection { get; }
        internal Dictionary<Type, TableData> Tables = new Dictionary<Type, TableData>();
        public Database(DbConnection conn, LogFactory factory = null)
        {
            Connection = conn;
            if (factory == null)
                factory = new LogFactory(new LoggingConfiguration());
            Container.OnTypeNotFound = t => throw new PerfusionException("Type not found: " + t);
            Container.AddInstance<LogFactory>(factory);
            Container.Add<LogInfo>();
            Container.AddInfo<ILogger>(Container.GetInstance<LogInfo>());
            Container.Add<SqlBuilder>();
            Container.Add<QuerySolver>();
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