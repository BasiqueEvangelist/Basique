using System.Data.Common;
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
    public interface IRelationLike
    {
        string Name { get; }
        void FillSet(ColumnSet set, QueryContext ctx);
        IQueryRelation MintLogical();
    }

    public interface IRelation : IAsyncQueryable, IRelationLike
    {
        BasiqueSchema Schema { get; }
    }

    //   
    public abstract class RelationBase<T> : IRelation, IAsyncQueryable<T>
    {
        public BasiqueSchema Schema { get; }

        protected RelationBase(BasiqueSchema conn)
        {
            Schema = conn;
        }

        public abstract string Name { get; }
        public Type ElementType => typeof(T);

        public Expression Expression => Expression.Constant(this);

        public IAsyncQueryProvider Provider => new BasiqueQueryProvider(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new BasiqueQueryable<T>(this, Expression).GetAsyncEnumerator(cancellationToken);
        public abstract void FillSet(ColumnSet set, QueryContext ctx);
        public abstract IQueryRelation MintLogical();
    }

    public class TableBase<T> : RelationBase<T>
    {
        protected readonly TableData data;
        public TableBase(BasiqueSchema conn) : base(conn)
        {
            data = conn.Tables[typeof(T)];
        }
        public override string Name => data.Name;

        public override void FillSet(ColumnSet set, QueryContext ctx)
        {
            foreach (var (path, column) in data.Columns)
            {
                set.Set(path, new BasiqueColumn()
                {
                    From = ctx.GetLogical(this),
                    Column = column
                });
            }
        }

        public override IQueryRelation MintLogical() => new DirectQueryRelation(this);
    }
}




