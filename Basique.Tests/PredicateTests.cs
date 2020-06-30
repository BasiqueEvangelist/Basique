using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class PredicateTests : TestEnvironment
    {
        public PredicateTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task BinaryOperations()
        {
            TestObject[] objects = await Db.TestObjects
                .Where(x => (x.Value > 3 || x.Value < 1) && (x.Test != "qux" || x.Test == "quux") ^ (x.Value == 5) || (x.Value >= 1 && x.Value <= 5) || (x.Value + 1 * 2 / 3 - 4 % 5) == 1)
                .ToArrayAsync();
        }

        [Fact]
        public async Task UnaryOperations()
        {
            TestObject[] objects = await Db.TestObjects
                .Where(x => !(x.Value == 1))
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" }
            });
        }

        [Fact]
        public async Task Ternary()
        {
            TestObject[] objects = await Db.TestObjects
                .Where(x => x.Test == "quux" ? x.Value == 5 : x.Value < 1)
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 5, Test = "quux" }
            });
        }
    }
}