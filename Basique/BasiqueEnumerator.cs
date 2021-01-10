using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Data;
using Basique.Solve;
using System.Runtime.Serialization;
using Basique.Services;

namespace Basique
{
    public class BasiqueEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly DbDataReader reader;
        private readonly CancellationToken token;
        private readonly DbConnection connection;
        private readonly PathTree<BasiqueColumn> columns;
        private readonly bool disposeConnection;

        public BasiqueEnumerator(DbDataReader reader, CancellationToken token, DbConnection connection, PathTree<BasiqueColumn> columns, bool disposeConnection)
        {
            this.reader = reader;
            this.token = token;
            this.connection = connection;
            this.columns = columns;
            this.disposeConnection = disposeConnection;
        }

        public T Current { get; private set; }

        public async ValueTask DisposeAsync()
        {
            await reader.DisposeAsync();
            if (disposeConnection)
                await connection.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!reader.HasRows) return false;
            if (!await reader.ReadAsync(token))
                return false;
            var newSet = new PathTree<object>();
            foreach (var (path, column) in columns.WalkValues())
            {
                object val = Convert.ChangeType(reader.GetValue(column.NamedAs), column.Column.Type);
                newSet.Set(path, val);
            }
            Current = (T)ObjectFactory.Create(typeof(T), newSet);
            return true;
        }
    }
}