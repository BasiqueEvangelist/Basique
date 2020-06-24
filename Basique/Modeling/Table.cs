using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Basique.Modeling
{
    public interface ITable : IAsyncQueryable
    {
        string Name { get; }
        DatabaseContext Context { get; }
    }
    public class Table<T> : IAsyncQueryable<T>, ITable
    {
        private readonly TableData data;

        public Table(DatabaseContext conn)
        {
            Context = conn;
            data = conn.tables[typeof(T)];
        }

        public string Name => data.Name;

        public DatabaseContext Context { get; }

        public Type ElementType => typeof(T);

        public Expression Expression => Expression.Constant(this);

        public IAsyncQueryProvider Provider => new BasiqueQueryProvider(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new BasiqueQueryable<T>(this, Expression).GetAsyncEnumerator(cancellationToken);
    }
}