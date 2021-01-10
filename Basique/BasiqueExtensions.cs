using System.Data.Common;
using System.Threading;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Basique.Modeling;
using Basique.Solve;
using System.Linq;
using Basique.Flattening;

namespace Basique
{
    public static class BasiqueExtensions
    {
        public static ValueTask<T> CreateAsync<T>(this Table<T> table, Expression<Func<T>> factory, CancellationToken token = default, BasiqueTransaction transaction = null)
         => table.WithTransaction(transaction).CreateAsyncInternal(factory, token);

        internal static ValueTask<T> CreateAsyncInternal<T>(this IAsyncQueryable<T> q, Expression<Func<T>> factory, CancellationToken token)
         => q.Provider.ExecuteAsync<T>(Expression.Call(null, KnownMethods.CreateAsyncInternal.MakeGenericMethod(typeof(T)), q.Expression, factory, Expression.Constant(token)), token);

        public static async ValueTask DeleteAsync<T>(this IAsyncQueryable<T> q, CancellationToken token = default)
         => await q.Provider.ExecuteAsync<object>(Expression.Call(null, KnownMethods.DeleteAsync.MakeGenericMethod(typeof(T)), q.Expression, Expression.Constant(token)), token);

        public static IAsyncQueryable<T> WithTransaction<T>(this RelationBase<T> rel, BasiqueTransaction trans)
         => rel.Provider.CreateQuery<T>(Expression.Call(null, KnownMethods.WithTransaction.MakeGenericMethod(typeof(T)), rel.Expression, Expression.Constant(trans, typeof(BasiqueTransaction))));

        public static Join<TLeft, TRight, TResult> SqlJoin<TLeft, TRight, TResult>(this RelationBase<TLeft> rel, RelationBase<TRight> other, Expression<Predicate<TResult>> on, Expression<Func<TLeft, TRight, TResult>> factory)
         => new(rel.Schema, rel, other, PredicateFlattener.Flatten(on.Body, on.Parameters), factory);
    }
}