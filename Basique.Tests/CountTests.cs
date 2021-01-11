using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class CountTests : TestEnvironment
    {
        public CountTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TotalCount()
        {
            var count = await Db.TestObjects.CountAsync();

            Assert.Equal(6, count);
        }

        [Fact]
        public async Task LongCount()
        {
            long count = await Db.TestObjects.LongCountAsync();

            Assert.Equal(6, count);
        }

        [Fact]
        public async Task PredicateCount()
        {
            var count = await Db.TestObjects.CountAsync(x => x.Value < 2);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task All()
        {
            Assert.True(await Db.TestObjects.AllAsync(x => x.Value < 7));
        }

        [Fact]
        public async Task FailingAll()
        {
            Assert.False(await Db.TestObjects.AllAsync(x => x.Value < 4));
        }

        [Fact]
        public async Task Any()
        {
            Assert.True(await Db.TestObjects.AnyAsync(x => x.Value == 0));
        }

        [Fact]
        public async Task FailingAny()
        {
            Assert.False(await Db.TestObjects.AnyAsync(x => x.Value > 7));
        }
    }
}