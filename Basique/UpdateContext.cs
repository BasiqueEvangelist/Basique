using System.Threading;
using System.Linq.Expressions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Basique.Solve;
using System.Threading.Tasks;
using Basique.Services;
using Basique.Flattening;

namespace Basique
{
    public static class UpdateQueryableExtensions
    {
        public static UpdateContext<T> Update<T>(this IAsyncQueryable<T> queryable) => new(queryable);
        internal static async ValueTask Commit<T>(this IAsyncQueryable<T> queryable, UpdateContext<T> ctx, CancellationToken tok = default)
        {
            await queryable.Provider.ExecuteAsync<object>(Expression.Call(null, KnownMethods.Commit.MakeGenericMethod(typeof(T)), queryable.Expression, Expression.Constant(ctx), Expression.Constant(tok)), tok);
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
            Data.Add((MemberPath.Create(selector.Body).Path, PredicateFlattener.Flatten(data.Body)));
            return this;
        }

        public ValueTask Commit(CancellationToken token = default)
            => Queryable.Commit(this, token);
    }
}