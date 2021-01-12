using System.Threading;
using System.Data.Common;
using System;
using System.Threading.Tasks;

namespace Basique
{
    public class BasiqueTransaction : IAsyncDisposable
    {
        internal readonly DbTransaction wrapping;
        private DbConnection connection;
        private bool commited;

        internal BasiqueTransaction(DbTransaction on, DbConnection connection)
        {
            wrapping = on;
            this.connection = connection;
        }

        public async Task Commit(CancellationToken token = default)
        {
            await wrapping.CommitAsync(token);
            commited = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (!commited)
                await wrapping.RollbackAsync();
            await wrapping.DisposeAsync();
            await connection.DisposeAsync();
        }

        public async Task<int> NonQuery(string query)
        {
            await using DbCommand comm = connection.CreateCommand();
            comm.CommandText = query;
            comm.Transaction = wrapping;
            return await comm.ExecuteNonQueryAsync();
        }
    }
}