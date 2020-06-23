using System.Threading;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HerringORM.Modeling;
using HerringORM.Solve;

namespace HerringORM
{
    public static class TableExtensions
    {
        public static ValueTask<T> CreateAsync<T>(this Table<T> table, Expression<Func<T>> factory, CancellationToken token = default(CancellationToken))
         => table.Provider.ExecuteAsync<T>(Expression.Call(null, KnownMethods.CreateAsync.MakeGenericMethod(typeof(T)), table.Expression, factory, Expression.Constant(token)), token);
    }
}