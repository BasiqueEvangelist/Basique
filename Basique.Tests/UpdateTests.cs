using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class UpdateTests : TestEnvironment
    {
        public UpdateTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Basic()
        {
            await Db.TestObjects
                .Update()
                .Set(x => x.Test, x => "bee")
                .Set(x => x.Value, x => 10)
                .Commit();

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 10, Test = "bee" },
                new TestObject() { Value = 10, Test = "bee" },
                new TestObject() { Value = 10, Test = "bee" },
                new TestObject() { Value = 10, Test = "bee" },
                new TestObject() { Value = 10, Test = "bee" },
                new TestObject() { Value = 10, Test = "bee" }
            });
        }

        [Fact]
        public async Task BasicWhere()
        {
            await Db.TestObjects
                .Where(x => x.Value > 3)
                .Update()
                .Set(x => x.Test, x => "bee")
                .Set(x => x.Value, x => 10)
                .Commit();

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 10, Test = "bee" },
                new TestObject() { Value = 10, Test = "bee" }
            });
        }

        [Fact]
        public async Task UpdateNullable()
        {
            await Db.TestObjects
                .Where(x => x.Value > 3)
                .Update()
                .Set(x => x.NullableValue, x => null)
                .Commit();

            await Db.TestObjects
                .Where(x => x.Value < 3)
                .Update()
                .Set(x => x.NullableValue, x => 10)
                .Commit();
        }
    }
}