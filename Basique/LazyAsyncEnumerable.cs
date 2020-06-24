using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Basique
{
    public class LazyAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly Func<ValueTask<IEnumerable<T>>> factory;

        public LazyAsyncEnumerable(Func<ValueTask<IEnumerable<T>>> factory)
        {
            this.factory = factory;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(factory);
        }
        public class Enumerator : IAsyncEnumerator<T>
        {
            private readonly Func<ValueTask<IEnumerable<T>>> factory;
            private IEnumerator<T> enumer = null;

            internal Enumerator(Func<ValueTask<IEnumerable<T>>> factory)
            {
                this.factory = factory;
            }

            public T Current => enumer == null ? default(T) : enumer.Current;

            public ValueTask DisposeAsync()
            {
                if (enumer != null) enumer.Dispose();
                return new ValueTask(Task.CompletedTask);
            }

            public ValueTask<bool> MoveNextAsync()
            {
                async ValueTask<bool> getEnumerator()
                {
                    enumer = (await factory()).GetEnumerator();
                    return enumer.MoveNext();
                }
                if (enumer != null)
                    return new ValueTask<bool>(enumer.MoveNext());
                else
                    return getEnumerator();
            }
        }
    }
}