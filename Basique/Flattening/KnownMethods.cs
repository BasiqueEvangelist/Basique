using System.Threading;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Basique.Modeling;
using System.Linq.Expressions;

namespace Basique.Flattening
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
        public static MethodInfo ThenBy = new Func<IOrderedAsyncQueryable<object>, Expression<Func<object, object>>, IAsyncQueryable<object>>(AsyncQueryable.ThenBy).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo ThenByDescending = new Func<IOrderedAsyncQueryable<object>, Expression<Func<object, object>>, IAsyncQueryable<object>>(AsyncQueryable.ThenByDescending).GetMethodInfo().GetGenericMethodDefinition();
        #region PullSingle variants
        public static MethodInfo FirstAsyncAlways = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask<object>>(AsyncQueryable.FirstAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo SingleAsyncAlways = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask<object>>(AsyncQueryable.SingleAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo FirstOrDefaultAsyncAlways = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask<object>>(AsyncQueryable.FirstOrDefaultAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo SingleOrDefaultAsyncAlways = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask<object>>(AsyncQueryable.SingleOrDefaultAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo FirstAsyncPredicate = new Func<IAsyncQueryable<object>, Expression<Func<object, bool>>, CancellationToken, ValueTask<object>>(AsyncQueryable.FirstAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo SingleAsyncPredicate = new Func<IAsyncQueryable<object>, Expression<Func<object, bool>>, CancellationToken, ValueTask<object>>(AsyncQueryable.SingleAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo FirstOrDefaultAsyncPredicate = new Func<IAsyncQueryable<object>, Expression<Func<object, bool>>, CancellationToken, ValueTask<object>>(AsyncQueryable.FirstOrDefaultAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo SingleOrDefaultAsyncPredicate = new Func<IAsyncQueryable<object>, Expression<Func<object, bool>>, CancellationToken, ValueTask<object>>(AsyncQueryable.SingleOrDefaultAsync).GetMethodInfo().GetGenericMethodDefinition();

        public static MethodInfo[] PullSingleVariants = new MethodInfo[] {
            FirstAsyncAlways, FirstAsyncPredicate, FirstOrDefaultAsyncAlways, FirstOrDefaultAsyncPredicate,
            SingleAsyncAlways, SingleAsyncPredicate, SingleOrDefaultAsyncAlways, SingleOrDefaultAsyncPredicate
        };

        public static MethodInfo[] PullSinglePredicated = new MethodInfo[] {
            FirstAsyncPredicate, FirstOrDefaultAsyncPredicate,
            SingleAsyncPredicate, SingleOrDefaultAsyncPredicate
        };

        public static MethodInfo[] PullSingleDefault = new MethodInfo[] {
            FirstOrDefaultAsyncAlways, FirstOrDefaultAsyncPredicate,
            SingleOrDefaultAsyncAlways, SingleOrDefaultAsyncPredicate
        };

        public static MethodInfo[] PullSingleFirst = new MethodInfo[]{
            FirstAsyncAlways, FirstAsyncPredicate, FirstOrDefaultAsyncAlways, FirstOrDefaultAsyncPredicate,
        };
        public static MethodInfo[] PullSingleSingle = new MethodInfo[]{
            SingleAsyncAlways, SingleAsyncPredicate, SingleOrDefaultAsyncAlways, SingleOrDefaultAsyncPredicate
        };
        #endregion

        public static MethodInfo CreateAsyncInternal = new Func<Table<object>, Expression<Func<object>>, CancellationToken, ValueTask<object>>(BasiqueExtensions.CreateAsyncInternal).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo DeleteAsync = new Func<IAsyncQueryable<object>, CancellationToken, ValueTask>(BasiqueExtensions.DeleteAsync).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo Commit = new Func<IAsyncQueryable<object>, UpdateContext<object>, CancellationToken, ValueTask>(UpdateQueryableExtensions.Commit).GetMethodInfo().GetGenericMethodDefinition();
        public static MethodInfo WithTransaction = new Func<RelationBase<object>, BasiqueTransaction, IAsyncQueryable<object>>(BasiqueExtensions.WithTransaction).GetMethodInfo().GetGenericMethodDefinition();
    }
}