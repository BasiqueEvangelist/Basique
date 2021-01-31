using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class ConstantFoldTests : TestEnvironment
    {
        public ConstantFoldTests(ITestOutputHelper output) : base(output)
        {
        }

        class CallTracker
        {
            public bool Called;

            public int GetValue()
            {
                Called = true;
                return 1;
            }
        }

        [Fact]
        public async Task FoldCall()
        {
            var tracker = new CallTracker();

            TestObject[] objects = await Db.TestObjects
                .Where(x => !(x.Value == tracker.GetValue()))
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" }
            });
            Assert.True(tracker.Called);
        }

        private static int staticField = 1;

        [Fact]
        public async Task FoldStaticField()
        {
            TestObject[] objects = await Db.TestObjects
                .Where(x => !(x.Value == staticField))
                .ToArrayAsync();

            Assert.Equal(objects, new TestObject[] {
                new TestObject() { Value = 0, Test = "oof" },
                new TestObject() { Value = 2, Test = "bar" },
                new TestObject() { Value = 3, Test = "baz" },
                new TestObject() { Value = 4, Test = "qux" },
                new TestObject() { Value = 5, Test = "quux" }
            });
        }
    }
}