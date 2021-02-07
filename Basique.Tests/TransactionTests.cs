using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class TransactionTests : TestEnvironment
    {
        public TransactionTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task RollbackCreate()
        {
            await using (var transaction = await Db.MintTransaction())
            {
                await Db.TestObjects
                    .Create(() => new TestObject() { Test = "beep", Value = 100 })
                    .WithTransaction(transaction)
                    .VoidAsync();
                // await transaction.Commit();
            }

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" }
            });
        }

        [Fact]
        public async Task RollbackDelete()
        {
            await using (var transaction = await Db.MintTransaction())
            {
                await Db.TestObjects
                    .WithTransaction(transaction)
                    .Where(x => x.Value > 3)
                    .DeleteAsync();
                // await transaction.Commit();
            }

            TestObject[] objects = await Db.TestObjects.ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" }
            });
        }
    }
}