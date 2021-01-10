using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class PullSingleTests : TestEnvironment
    {
        public PullSingleTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task BasicFirstView()
        {
            TestJoin obj = await Db.TestJoin.FirstAsync();

            Assert.Equal(0, obj.First.Value);
            Assert.Equal("oof", obj.Second.Test);
        }

        [Fact]
        public async Task BasicFirstJoin()
        {
            var obj = await Db.TestObjects.SqlJoin(Db.TestObjects, (pair) => pair.First.Value == pair.Second.Value, (a, b) => new { First = a, Second = b }).FirstAsync();

            Assert.Equal(0, obj.First.Value);
            Assert.Equal("oof", obj.Second.Test);
        }

        [Fact]
        public async Task BasicFirst()
        {
            TestObject obj = await Db.TestObjects.FirstAsync();

            Assert.Equal(obj, new TestObject() { Value = 0, Test = "oof" });
        }

        [Fact]
        public async Task BasicSingle()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await Db.TestObjects.SingleAsync());
        }

        [Fact]
        public async Task EmptyFirst()
        {
            await Db.TestObjects.DeleteAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await Db.TestObjects.FirstAsync());
        }

        [Fact]
        public async Task EmptySingle()
        {
            await Db.TestObjects.DeleteAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await Db.TestObjects.SingleAsync());
        }
    }
}