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
    }
}