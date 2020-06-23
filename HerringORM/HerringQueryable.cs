using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using HerringORM.Modeling;

namespace HerringORM
{
    public class HerringQueryable<T> : IAsyncQueryable<T>
    {
        private readonly ITable tab;

        internal HerringQueryable(ITable tab, Expression expression)
        {
            if (expression.Type != typeof(IAsyncQueryable<T>))
                throw new ArgumentException("Expression must be of query type", nameof(expression));
            this.tab = tab;
            Expression = expression;
        }

        public Type ElementType => GetType();

        public Expression Expression { get; }

        public IAsyncQueryProvider Provider => new HerringQueryProvider(tab);
        // public IAsyncQueryProvider Provider => new WorstQueryProvider();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new LazyAsyncEnumerable<T>(
                async () => await this.ToArrayAsync(cancellationToken)
            ).GetAsyncEnumerator();
    }
}
