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

        [Fact]
        public void Prepend()
        {
            Expression<Func<A, A>> expr1 = (a) => a.a.c;
            Expression<Func<A.B, A>> expr2 = (b) => b.c;
            Assert.Equal(MemberPath.Create(expr1.Body).Path, MemberPath.Create(expr2.Body).Path.Prepend(typeof(A).GetField("a")));
        }

        [Fact]
        public void CanFollowType()
        {
            Expression<Func<A, A>> expr = (a) => a.a.c;
            Assert.True(MemberPath.Create(expr.Body).Path.CanFollowType(typeof(A)));
        }

        [Fact]
        public void CantFollowType()
        {
            Expression<Func<A, A>> expr = (a) => a.a.c;
            Assert.False(MemberPath.Create(expr.Body).Path.CanFollowType(typeof(A.B)));
        }
    }
}