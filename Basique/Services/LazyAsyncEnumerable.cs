using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Basique.Services
{
    public class LazyAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly Func<ValueTask<IAsyncEnumerator<T>>> factory;
        private IAsyncEnumerator<T> enumer = null;

        internal LazyAsyncEnumerator(Func<ValueTask<IAsyncEnumerator<T>>> factory)
        {
            this.factory = factory;
        }

        public T Current => enumer == null ? default(T) : enumer.Current;

        public ValueTask DisposeAsync()
            => enumer != null ? enumer.DisposeAsync() : new ValueTask(Task.CompletedTask);

        public ValueTask<bool> MoveNextAsync()
        {
            async ValueTask<bool> getEnumerator()
            {
                enumer = await factory();
                return await enumer.MoveNextAsync();
            }
            if (enumer != null)
                return enumer.MoveNextAsync();
            else
                return getEnumerator();
        }
    }
}