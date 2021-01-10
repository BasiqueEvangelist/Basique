using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class DeleteTests : TestEnvironment
    {
        public DeleteTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task FullTable()
        {
            await Db.TestObjects.DeleteAsync();

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, Array.Empty<TestObject>());
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