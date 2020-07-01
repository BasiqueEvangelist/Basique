using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using Basique.Modeling;
using System.Data;

namespace Basique
{
    public class BasiqueEnumerator<T> : IAsyncEnumerator<T> where T : new()
    {
        private readonly DbDataReader reader;
        private readonly IRelation rel;
        private readonly CancellationToken token;
        private readonly DbConnection connection;
        private readonly bool disposeConnection;

        public BasiqueEnumerator(DbDataReader reader, CancellationToken token, IRelation rel, DbConnection connection, bool disposeConnection)
        {
            this.reader = reader;
            this.token = token;
            this.rel = rel;
            this.connection = connection;
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
            if (!(await reader.ReadAsync(token)))
                return false;
            T obj = new T();
            foreach (var columnData in rel.Schema.Tables[typeof(T)].Columns.Values)
            {
                object val = Convert.ChangeType(reader.GetValue(columnData.Name), columnData.Type);
                columnData.Path.Set(obj, val);
            }
            Current = (T)obj;
            return true;
        }
    }
}