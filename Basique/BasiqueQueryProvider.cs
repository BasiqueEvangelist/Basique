using System.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Basique.Modeling;
using Basique.Solve;
using NLog;

namespace Basique
{
    public class BasiqueQueryProvider : IAsyncQueryProvider
    {
        private static Logger LOGGER = LogManager.GetCurrentClassLogger();
        private ITable tab;

        public BasiqueQueryProvider(ITable tab)
        {
            this.tab = tab;
        }

        public IAsyncQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new BasiqueQueryable<TElement>(tab, expression);
        }

        public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            LOGGER.Trace("Expr: {0}", expression);
            List<ExpressionNode> pn = ToplevelExpressionFlattener.ParseAndFlatten(expression);
            pn.Last().Dump();
            if (pn.Last() is PullExpressionNode)
                return (TResult)await QuerySolver.SolvePullQuery(pn, token, tab);
            else if (pn.Last() is CreateExpressionNode)
                return (TResult)await QuerySolver.SolveCreateQuery(pn, token, tab);
            else if (pn.Last() is UpdateExpressionNode)
                return (TResult)await QuerySolver.SolveUpdateQuery(pn, token, tab);
            else
                throw new NotImplementedException();
        }
    }
}