using System.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Basique.Solve;
using NLog;
using NLog.Config;

namespace Basique.Modeling
{
    public abstract class Database
    {
        public class Services
        {
            public SqlBuilder SqlBuilder;
            public QuerySolver Solver;
            public LogFactory LoggerFactory;
        }

        public Services LoadedServices { get; private set; } = new Services();
        public DbConnection Connection { get; }
        internal Dictionary<Type, TableData> Tables = new Dictionary<Type, TableData>();
        public Database(DbConnection conn, LogFactory factory = null)
        {
            Connection = conn;
            if (factory == null)
                factory = new LogFactory(new LoggingConfiguration());
            LoadedServices.LoggerFactory = factory;
            LoadedServices.SqlBuilder = new SqlBuilder();
            LoadedServices.Solver = new QuerySolver(LoadedServices.LoggerFactory, LoadedServices.SqlBuilder);
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