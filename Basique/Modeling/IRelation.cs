using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Basique.Solve;

namespace Basique.Modeling
{
    public interface IRelation : IAsyncQueryable
    {
        string Name { get; }
        BasiqueSchema Schema { get; }
    }

    //   
    public abstract class RelationBase<T> : IRelation, IAsyncQueryable<T>
    {
        protected readonly TableData data;
        public BasiqueSchema Schema { get; }

        protected RelationBase(BasiqueSchema conn)
        {
            Schema = conn;
            data = conn.Tables[typeof(T)];
        }

        public string Name => data.Name;

        public Type ElementType => typeof(T);

        public Expression Expression => Expression.Constant(this);

        public IAsyncQueryProvider Provider => new BasiqueQueryProvider(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new BasiqueQueryable<T>(this, Expression).GetAsyncEnumerator(cancellationToken);
    }
}




