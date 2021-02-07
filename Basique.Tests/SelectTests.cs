using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class SelectTests : TestEnvironment
    {
        public SelectTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task BasicSelect()
        {
            var obj = await Db.TestObjects.Select(x => new { AnonValue = x.Value, AnonTest = x.Test }).FirstAsync();

            Assert.Equal("oof", obj.AnonTest);
            Assert.Equal(0, obj.AnonValue);
        }

        [Fact]
        public async Task BasicSelectOne()
        {
            var obj = await Db.TestObjects.Select(x => x.Test).FirstAsync();

            Assert.Equal("oof", obj);
        }

        [Fact]
        public async Task SelectPull()
        {
            var arr = await Db.TestObjects.Select(x => new { AnonValue = x.Value, AnonTest = x.Test }).Where(x => x.AnonValue == 0).ToArrayAsync();

            Assert.Single(arr);
            Assert.Equal("oof", arr[0].AnonTest);
            Assert.Equal(0, arr[0].AnonValue);
        }

        [Fact]
        public async Task SelectJoin()
        {
            var obj = await Db.TestObjects.SqlJoin(Db.TestObjects, (pair) => pair.First.Value == pair.Second.Value, (a, b) => new { First = a, Second = b }).Select(x => x.First).FirstAsync();

            Assert.Equal(0, obj.Value);
            Assert.Equal("oof", obj.Test);
        }
    }
}