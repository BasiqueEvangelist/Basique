using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Basique.Modeling
{
    public class Table<T> : RelationBase<T>
    {
        public Table(BasiqueSchema conn)
            : base(conn)
        {

        }

    }
}