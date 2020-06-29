using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Basique.Modeling;
using Basique.Solve;
using System;

namespace Basique
{
    public class BasiqueQueryProvider : IAsyncQueryProvider
    {
        private readonly IRelation relation;
        private readonly DbTransaction transaction;

        internal BasiqueQueryProvider(IRelation relation, DbTransaction trans)
        {
            this.relation = relation;
            this.transaction = trans;
        }

        public IAsyncQueryable<TElement> CreateQuery<TElement>(Expression expression)
                    => new BasiqueQueryable<TElement>(relation, expression, transaction);

        public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            List<ExpressionNode> pn = ToplevelExpressionFlattener.ParseAndFlatten(expression);
            pn.Last().Dump();
            if (pn.Last() is PullExpressionNode)
                return (TResult)await QuerySolver.SolvePullQuery(pn, token, relation, transaction);
            else if (pn.Last() is CreateExpressionNode)
                return (TResult)await QuerySolver.SolveCreateQuery(pn, token, relation, transaction);
            else if (pn.Last() is UpdateExpressionNode)
                return (TResult)await QuerySolver.SolveUpdateQuery(pn, token, relation, transaction);
            else if (pn.Last() is DeleteExpressionNode)
                return (TResult)await QuerySolver.SolveDeleteQuery(pn, token, relation, transaction);
            else if (pn.Last() is PullSingleExpressionNode)
                return (TResult)await QuerySolver.SolvePullSingleQuery(pn, token, relation, transaction);
            else
                throw new NotImplementedException();
        }
    }
}