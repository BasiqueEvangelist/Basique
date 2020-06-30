using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Basique.Modeling;
using Basique.Services;
using Basique.Solve;

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

        public Type ElementType => GetType();

        public Expression Expression { get; }

        public IAsyncQueryProvider Provider => new BasiqueQueryProvider(tab);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new LazyAsyncEnumerator<T>(() => Provider.ExecuteAsync<IAsyncEnumerator<T>>(Expression, cancellationToken));
    }
}
