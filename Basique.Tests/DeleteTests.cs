using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Basique.Tests
{
    public class DeleteTests : TestEnvironment
    {
        [Fact]
        public async Task FullTable()
        {
            await Db.TestObjects.DeleteAsync();

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[0]);
        }

        [Fact]
        public async Task SimpleWhere()
        {
            await Db.TestObjects
                .Where(x => x.Value > 3)
                .DeleteAsync();

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" }
            });
        }
    }
}