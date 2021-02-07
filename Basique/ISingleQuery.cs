using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Basique
{
    public interface IAsyncSingleQueryProvider : IAsyncQueryProvider
    {
        ISingleQuery<TElement> CreateSingleQuery<TElement>(Expression expression);
    }

    public interface ISingleQuery
    {
        Type ResultType
        {
            get;
        }

        Expression Expression
        {
            get;
        }

        IAsyncSingleQueryProvider Provider
        {
            get;
        }
    }

    public interface ISingleQuery<T> : ISingleQuery
    {
        ValueTask<T> RunAsync(CancellationToken token = default);
    }
}