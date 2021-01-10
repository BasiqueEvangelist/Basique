using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Basique.Solve;

namespace Basique.Modeling
{
    public class View<T> : TableBase<T>
    {
        public View(BasiqueSchema conn) : base(conn)
        {
        }
    }
}
