using System.Security.AccessControl;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Basique.Solve;
using Basique.Services;
using Basique.Conversion;

namespace Basique.Modeling
{
    public abstract class BasiqueSchema
    {
        public Func<DbConnection> ConnectionFactory { get; }
        public IBasiqueLogger Logger { get; set; } = new EmptyLogger();
        public SqlGenerationSettings SqlGeneration { get; }
        internal Dictionary<Type, TableData> Tables = new();
        public StackConverter Converter = new();
        public BasiqueSchema(Func<DbConnection> conn, SqlGenerationSettings sqlGen)
        {
            ConnectionFactory = conn;
            SqlGeneration = sqlGen;

            Converter.Stack.Add(new ConvertConverter());
            Converter.Stack.Add(new NullableWrapConverter(Converter));
        }

        public async Task<BasiqueTransaction> MintTransaction(CancellationToken token = default)
        {
            DbConnection conn = await MintConnection(token);
            return new BasiqueTransaction(await conn.BeginTransactionAsync(token), conn);
        }

        // Probably should be replaced by some kind of pooling.
        public async Task<DbConnection> MintConnection(CancellationToken token = default)
        {
            DbConnection conn = ConnectionFactory();
            await conn.OpenAsync(token);
            return conn;
        }

        protected void Table<T>(Action<TableBuilder<T>> action)
        {
            TableData d = new();
            action(new TableBuilder<T>(d));
            Tables.Add(typeof(T), d);
        }
    }
}