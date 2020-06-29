using System.Threading;
using System.Data.Common;
using System;
using System.Threading.Tasks;

namespace Basique
{
    public class BasiqueTransaction : IAsyncDisposable
    {
        internal readonly DbTransaction wrapping;
        private bool commited;

        internal BasiqueTransaction(DbTransaction on)
        {
            wrapping = on;
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
        }
    }
}