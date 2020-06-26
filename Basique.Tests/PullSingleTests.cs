using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Basique.Tests
{
    public class PullSingleTests : TestEnvironment
    {
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