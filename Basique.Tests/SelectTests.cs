using System.Security.AccessControl;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Basique.Tests
{
    public class SelectTests : TestEnvironment
    {
        [Fact]
        public async Task FullTable()
        {
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
        public async Task OrderBy()
        {
            TestObject[] objects = await Db.TestObjects
                .OrderByDescending(x => x.Value)
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 5, Test = "quux" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 0, Test = "oof" }
            });
        }

        [Fact]
        public async Task ThenBy()
        {
            TestObject[] objects = await Db.TestObjects
                .OrderByDescending(x => x.Value + 1)
                .ThenByDescending(x => x.Test)
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 5, Test = "quux" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 1, Test = "foo" },
                new TestObject() { Value = 0, Test = "oof" }
            });
        }

        [Fact]
        public async Task SimpleWhere()
        {
            TestObject[] objects = await Db.TestObjects
                .Where(x => x.Value > 3)
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" }
            });
        }

        [Fact]
        public async Task WithTransaction()
        {
            await using (var transaction = await Db.BeginTransaction())
            {
                TestObject[] objects = await Db.TestObjects.WithTransaction(transaction).ToArrayAsync();

                Assert.Equal(objects, new TestObject[] {
                    new TestObject() { Value = 0, Test = "oof" },
                    new TestObject() { Value = 1, Test = "foo" },
                    new TestObject() { Value = 2, Test = "bar" },
                    new TestObject() { Value = 3, Test = "baz" },
                    new TestObject() { Value = 4, Test = "qux" },
                    new TestObject() { Value = 5, Test = "quux" }
                });

                await transaction.Commit();
            }
        }
    }
}