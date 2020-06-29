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
    public interface IRelation : IAsyncQueryable, IAsyncQueryProvider
    {
        string Name { get; }
        Database Context { get; }
    }

    //   
    public abstract class RelationBase<T> : IRelation, IAsyncQueryable<T>
    {
        protected readonly TableData data;
        public Database Context { get; }

        protected RelationBase(Database conn)
        {
            Context = conn;
            data = conn.Tables[typeof(T)];
        }

        public string Name => data.Name;

        public Type ElementType => typeof(T);

        public Expression Expression => Expression.Constant(this);

        public IAsyncQueryProvider Provider => this;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new BasiqueQueryable<T>(this, Expression).GetAsyncEnumerator(cancellationToken);

        public IAsyncQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new BasiqueQueryable<TElement>(this, expression);


        public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            List<ExpressionNode> pn = ToplevelExpressionFlattener.ParseAndFlatten(expression);
            pn.Last().Dump();
            if (pn.Last() is PullExpressionNode)
                return (TResult)await QuerySolver.SolvePullQuery(pn, token, this);
            else if (pn.Last() is CreateExpressionNode)
                return (TResult)await QuerySolver.SolveCreateQuery(pn, token, this);
            else if (pn.Last() is UpdateExpressionNode)
                return (TResult)await QuerySolver.SolveUpdateQuery(pn, token, this);
            else if (pn.Last() is DeleteExpressionNode)
                return (TResult)await QuerySolver.SolveDeleteQuery(pn, token, this);
            else if (pn.Last() is PullSingleExpressionNode)
                return (TResult)await QuerySolver.SolvePullSingleQuery(pn, token, this);
            else
                throw new NotImplementedException();
        }
    }
}




