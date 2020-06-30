using System.Linq;
using System;
using System.Linq.Expressions;
using Basique.Solve;
using Xunit;
using System.Reflection;
using Basique.Services;

namespace Basique.Tests.Internal
{
    public class MemberPathTests
    {
        class A
        {
            public B a;
            public class B
            {
                public A c;
            }
        }

        [Fact]
        public void Parameter()
        {
            Expression<Func<A, A>> expr = (a) => a.a.c;
            var path = MemberPath.Create(expr.Body).Path;
            Assert.Equal(new MemberPath() { Start = typeof(A), Members = new[] { typeof(A).GetField("a"), typeof(A.B).GetField("c") } }, path);
        }

        [Fact]
        public void Constant()
        {
            Expression<Func<int>> expr = () => "A".Length;
            var path = MemberPath.Create(expr.Body).Path;
            Assert.Equal(new MemberPath() { Start = typeof(string), Members = new MemberInfo[] { typeof(string).GetProperty("Length") } }, path);
        }
    }
}