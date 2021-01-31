using System.Threading;
using System.Linq.Expressions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Basique.Services;
using Basique.Flattening;

namespace Basique
{
    public static class UpdateQueryableExtensions
    {
        public static UpdateContext<T> Update<T>(this IAsyncQueryable<T> queryable) => new(queryable);
        internal static async ValueTask ApplyAsync<T>(this IAsyncQueryable<T> queryable, UpdateContext<T> ctx, CancellationToken tok = default)
        {
            await queryable.Provider.ExecuteAsync<object>(Expression.Call(null, KnownMethods.ApplyAsync.MakeGenericMethod(typeof(T)), queryable.Expression, Expression.Constant(ctx), Expression.Constant(tok)), tok);
        }
    }
    public abstract class UpdateContext
    {
        internal List<(MemberPath field, FlatPredicateNode factory)> Data { get; } = new List<(MemberPath field, FlatPredicateNode factory)>();
    }
    public class UpdateContext<T> : UpdateContext
    {
        public IAsyncQueryable<T> Queryable { get; }

        internal UpdateContext(IAsyncQueryable<T> queryable)
        {
            Queryable = queryable;
        }

        public UpdateContext<T> Set<TField>(Expression<Func<T, TField>> selector, Expression<Func<T, TField>> data)
        {
            Data.Add((MemberPath.Create(selector.Body).Path, PredicateFlattener.Flatten(data.Body, data.Parameters)));
            return this;
        }

        public UpdateContext<T> Set<TField>(Expression<Func<T, TField>> selector, TField data)
        {
            Data.Add((MemberPath.Create(selector.Body).Path, new ConstantPredicate(data)));
            return this;
        }

        public ValueTask ApplyAsync(CancellationToken token = default)
            => Queryable.ApplyAsync(this, token);
    }
}