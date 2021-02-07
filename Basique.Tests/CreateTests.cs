using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class CreateTests : TestEnvironment
    {
        public CreateTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Basic()
        {
            await Db.TestObjects.Create(() => new TestObject() { Test = "beep", Value = 100 }).VoidAsync();

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" },
                new TestObject() { Value = 100, Test = "beep" }
            });
        }

        [Fact]
        public async Task WithTransaction()
        {
            await using var transaction = await Db.MintTransaction();
            await Db.TestObjects
                .Create(() => new TestObject() { Test = "beep", Value = 100 })
                .WithTransaction(transaction)
                .VoidAsync();
            await transaction.Commit();
        }

        [Fact]
        public async Task ReturningCreate()
        {
            var obj = await Db.TestObjects
                .Create(() => new TestObject() { Test = "beep", Value = 100 })
                .RunAsync();
            Assert.Equal(obj, new TestObject() { Test = "beep", Value = 100 });
        }

        [Fact]
        public async Task ReturningCreateSelect()
        {
            var obj = await Db.TestObjects
                .Create(() => new TestObject() { Test = "beep", Value = 100 })
                .Select(x => new { AnonTest = x.Test, AnonValue = x.Value })
                .RunAsync();
            Assert.Equal("beep", obj.AnonTest);
            Assert.Equal(100, obj.AnonValue);
        }
    }
}