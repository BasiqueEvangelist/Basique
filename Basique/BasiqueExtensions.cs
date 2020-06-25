using System.Threading;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Basique.Modeling;
using Basique.Solve;
using System.Linq;

namespace Basique
{
    public static class BasiqueExtensions
    {
        public static ValueTask<T> CreateAsync<T>(this Table<T> table, Expression<Func<T>> factory, CancellationToken token = default(CancellationToken))
         => table.Provider.ExecuteAsync<T>(Expression.Call(null, KnownMethods.CreateAsync.MakeGenericMethod(typeof(T)), table.Expression, factory, Expression.Constant(token)), token);

        public static async ValueTask DeleteAsync<T>(this IAsyncQueryable<T> q, CancellationToken token = default(CancellationToken))
         => await q.Provider.ExecuteAsync<object>(Expression.Call(null, KnownMethods.DeleteAsync.MakeGenericMethod(typeof(T)), q.Expression, Expression.Constant(token)), token);
    }
}