using System.Threading;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Basique.Modeling;
using System.Linq.Expressions;

namespace Basique.Solve
{
    public static class KnownMethods
    {
        public static MethodInfo Where = new Func<IAsyncQueryable<object>, System.Linq.Expressions.Expression<System.Func<object, bool>>, IAsyncQueryable<object>>(AsyncQueryable.Where).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo Take = new Func<IAsyncQueryable<object>, int, IAsyncQueryable<object>>(AsyncQueryable.Take).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo Select = new Func<IAsyncQueryable<object>, System.Linq.Expressions.Expression<System.Func<object, object>>, IAsyncQueryable<object>>(AsyncQueryable.Select).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo ToListAsync = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask<List<object>>>(AsyncQueryable.ToListAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo ToArrayAsync = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask<object[]>>(AsyncQueryable.ToArrayAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo OrderBy = new Func<IAsyncQueryable<object>, Expression<Func<object, object>>, IAsyncQueryable<object>>(AsyncQueryable.OrderBy).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo OrderByDescending = new Func<IAsyncQueryable<object>, Expression<Func<object, object>>, IAsyncQueryable<object>>(AsyncQueryable.OrderByDescending).GetMethodInfo().GetGenericMethodDefinition();

        public static MethodInfo CreateAsync = new Func<Table<object>, Expression<Func<object>>, CancellationToken, ValueTask<object>>(BasiqueExtensions.CreateAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo DeleteAsync = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask>(BasiqueExtensions.DeleteAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo Commit = new Func<IAsyncQueryable<object>, UpdateContext<object>, CancellationToken, ValueTask>(UpdateQueryableExtensions.Commit).GetMethodInfo().GetGenericMethodDefinition();
    }
}