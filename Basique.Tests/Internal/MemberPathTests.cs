using System.Linq;
using System;
using System.Linq.Expressions;
using Basique.Solve;
using Xunit;
using System.Reflection;

namespace Basique.Tests.Internal
{
    public class MemberPathTests
    {
        class A
        {
            public A d;
        }

        [Fact]
        public void Parameter()
        {
            Expression<Func<A, A>> expr = (a) => a.d.d;
            var path = MemberPath.Create(expr.Body).Path;
            Assert.Equal(new MemberPath() { Start = typeof(A), Members = Enumerable.Repeat(typeof(A).GetField("d"), 2).ToArray() }, path);
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