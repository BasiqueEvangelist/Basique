using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Basique.Modeling;
using Basique.Services;

namespace Basique
{
    public class BasiqueQueryable<T> : IAsyncQueryable<T>, IOrderedAsyncQueryable<T>
    {
        private readonly IRelation tab;

        internal BasiqueQueryable(IRelation tab, Expression expression)
        {
            this.tab = tab;
            Expression = expression;
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IAsyncQueryProvider Provider => new BasiqueQueryProvider(tab);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new LazyAsyncEnumerator<T>(() => Provider.ExecuteAsync<IAsyncEnumerator<T>>(Expression, cancellationToken));
    }

    public class BasiqueSingleQuery<T> : ISingleQuery<T>
    {
        private readonly IRelation tab;

        internal BasiqueSingleQuery(IRelation tab, Expression expression)
        {
            this.tab = tab;
            Expression = expression;
        }

        public Type ResultType => typeof(T);

        public Expression Expression { get; }

        public IAsyncSingleQueryProvider Provider => new BasiqueQueryProvider(tab);

        public ValueTask<T> RunAsync(CancellationToken token = default)
            => BasiqueExtensions.RunAsync(this, token);
    }
}
