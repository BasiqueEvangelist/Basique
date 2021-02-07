using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Basique.Modeling;
using Basique.Solve;
using Basique.Flattening;
using Basique.Services;

namespace Basique
{
    public class BasiqueQueryProvider : IAsyncSingleQueryProvider
    {
        private readonly IRelation relation;

        internal BasiqueQueryProvider(IRelation relation)
        {
            this.relation = relation;
        }

        public IAsyncQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new BasiqueQueryable<TElement>(relation, expression);

        public ISingleQuery<TElement> CreateSingleQuery<TElement>(Expression expression)
            => new BasiqueSingleQuery<TElement>(relation, expression);

        public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            using var watcher = new Watcher("Query executed in ", relation.Schema.Logger);

            List<ExpressionNode> pn = ToplevelExpressionFlattener.ParseAndFlatten(expression);
            pn[^1].Dump(relation.Schema.Logger);
            if (pn.Last() is UpdateExpressionNode)
                return (TResult)await QuerySolver.SolveUpdateQuery(pn, token, relation);
            else if (pn.Last() is DeleteExpressionNode)
                return (TResult)await QuerySolver.SolveDeleteQuery(pn, token, relation);
            else if (pn.Last() is PullSingleExpressionNode)
                return (TResult)await QuerySolver.SolvePullSingleQuery(pn, token, relation);
            else if (pn.Last() is CountExpressionNode)
                return (TResult)await QuerySolver.SolveCountQuery(pn, token, relation);
            else if (pn.Any(x => x is CreateExpressionNode))
                return (TResult)await QuerySolver.SolveCreateQuery(pn, token, relation);
            else
                return (TResult)await QuerySolver.SolvePullQuery(pn, token, relation);
        }
    }
}