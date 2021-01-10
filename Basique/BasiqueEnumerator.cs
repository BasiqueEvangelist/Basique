using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Data;
using Basique.Solve;

namespace Basique
{
    public class BasiqueEnumerator<T> : IAsyncEnumerator<T> where T : new()
    {
        private readonly DbDataReader reader;
        private readonly CancellationToken token;
        private readonly DbConnection connection;
        private readonly ColumnSet columns;
        private readonly bool disposeConnection;

        public BasiqueEnumerator(DbDataReader reader, CancellationToken token, DbConnection connection, ColumnSet columns, bool disposeConnection)
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
            T obj = new T();
            foreach (var (path, column) in columns.WalkColumns())
            {
                object val = Convert.ChangeType(reader.GetValue(column.NamedAs), column.Column.Type);
                path.Set(obj, val);
            }
            Current = obj;
            return true;
        }
    }
}