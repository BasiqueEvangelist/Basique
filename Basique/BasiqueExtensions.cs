using System.Threading;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Basique.Modeling;
using System.Linq;
using Basique.Flattening;
using Basique.Solve;

namespace Basique
{
    public static class BasiqueExtensions
    {
        public static ISingleQuery<T> Create<T>(this Table<T> table, Expression<Func<T>> factory)
         => table.Provider.CreateSingleQuery<T>(Expression.Call(null, KnownMethods.Create.MakeGenericMethod(typeof(T)), table.Expression, factory));

        public static async ValueTask DeleteAsync<T>(this IAsyncQueryable<T> q, CancellationToken token = default)
         => await q.Provider.ExecuteAsync<object>(Expression.Call(null, KnownMethods.DeleteAsync.MakeGenericMethod(typeof(T)), q.Expression, Expression.Constant(token)), token);

        public static IAsyncQueryable<T> WithTransaction<T>(this IAsyncQueryable<T> rel, BasiqueTransaction trans)
         => rel.Provider.CreateQuery<T>(Expression.Call(null, KnownMethods.WithTransactionQueryable.MakeGenericMethod(typeof(T)), rel.Expression, Expression.Constant(trans, typeof(BasiqueTransaction))));

        public static ISingleQuery<T> WithTransaction<T>(this ISingleQuery<T> rel, BasiqueTransaction trans)
         => rel.Provider.CreateSingleQuery<T>(Expression.Call(null, KnownMethods.WithTransactionSingle.MakeGenericMethod(typeof(T)), rel.Expression, Expression.Constant(trans, typeof(BasiqueTransaction))));

        public static Join<TLeft, TRight, TResult> SqlJoin<TLeft, TRight, TResult>(this RelationBase<TLeft> rel, RelationBase<TRight> other, Expression<Predicate<TResult>> on, Expression<Func<TLeft, TRight, TResult>> factory)
         => new(rel.Schema, rel, other, PredicateFlattener.Flatten(on.Body, on.Parameters), factory);

        public static async ValueTask VoidAsync<T>(this ISingleQuery<T> query, CancellationToken token = default)
         => await query.Provider.ExecuteAsync<object>(Expression.Call(null, KnownMethods.VoidAsync.MakeGenericMethod(typeof(T)), query.Expression, Expression.Constant(token)), token);

        internal static ValueTask<T> RunAsync<T>(ISingleQuery<T> query, CancellationToken token = default)
         => query.Provider.ExecuteAsync<T>(Expression.Call(null, KnownMethods.RunAsync.MakeGenericMethod(typeof(T)), query.Expression, Expression.Constant(token)), token);

        public static ISingleQuery<TTo> Select<TFrom, TTo>(this ISingleQuery<TFrom> query, Expression<Func<TFrom, TTo>> selector)
         => query.Provider.CreateSingleQuery<TTo>(Expression.Call(null, KnownMethods.SelectSingle.MakeGenericMethod(typeof(TFrom), typeof(TTo)), query.Expression, selector));

        [MethodWriter(typeof(DefaultFunctionWriter))]
        public static bool SqlLike(this string source, string match)
         => throw new NotImplementedException("Managed SqlLike is not yet implemented.");
    }
}