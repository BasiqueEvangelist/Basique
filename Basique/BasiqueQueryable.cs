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
        private readonly DbTransaction transaction;

        internal BasiqueQueryable(IRelation tab, Expression expression, DbTransaction trans)
        {
            this.tab = tab;
            Expression = expression;
            transaction = trans;
        }

        public Type ElementType => GetType();

        public Expression Expression { get; }

        public IAsyncQueryProvider Provider => new BasiqueQueryProvider(tab, transaction);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new LazyAsyncEnumerator<T>(() => Provider.ExecuteAsync<IAsyncEnumerator<T>>(Expression, cancellationToken));
    }
}
