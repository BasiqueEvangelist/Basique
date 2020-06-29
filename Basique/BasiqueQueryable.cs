using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Basique.Modeling;

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

        public IAsyncQueryProvider Provider => tab;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new LazyAsyncEnumerable<T>(
                async () => await this.ToArrayAsync(cancellationToken)
            ).GetAsyncEnumerator();
    }
}
