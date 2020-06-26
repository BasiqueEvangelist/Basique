using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Basique.Modeling;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Basique.Tests
{
    public class TestEnvironment : IAsyncLifetime
    {
        public class TestObject
        {
            public string Test;
            public int Value;


            public override string ToString() => $"(\"{Test}\";{Value})";

            public override bool Equals(object input)
            {
                return input is TestObject obj &&
                       Test == obj.Test &&
                       Value == obj.Value;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Test, Value);
            }

            public static bool operator ==(TestObject a, TestObject b)
                => a is null ? b is null : a.Equals(b);

            public static bool operator !=(TestObject a, TestObject b)
                => !(a == b);
        }
        public class TestContext : DatabaseContext
        {
            public Table<TestObject> TestObjects => new Table<TestObject>(this);

            public TestContext(DbConnection conn) : base(conn)
            {
                Table<TestObject>(build =>
                {
                    build.RemoteName("testobjects");

                    build.Field(x => x.Test);
                    build.Field(x => x.Value);
                });
            }
        }

        protected TestContext Db;

        public async Task InitializeAsync()
        {
            SqliteConnection conn = new SqliteConnection(new SqliteConnectionStringBuilder("")
            {
                DataSource = ":memory:"
            }.ToString());
            await conn.OpenAsync();

            await conn.NonQuery("CREATE TABLE testobjects (test TEXT, value INT);");

            Db = new TestContext(conn);

            await Db.TestObjects.CreateAsync(() => new TestObject() { Value = 0, Test = "oof" });
            await Db.TestObjects.CreateAsync(() => new TestObject() { Value = 1, Test = "foo" });
            await Db.TestObjects.CreateAsync(() => new TestObject() { Value = 2, Test = "bar" });
            await Db.TestObjects.CreateAsync(() => new TestObject() { Value = 3, Test = "baz" });
            await Db.TestObjects.CreateAsync(() => new TestObject() { Value = 4, Test = "qux" });
            await Db.TestObjects.CreateAsync(() => new TestObject() { Value = 5, Test = "quux" });
        }

        public async Task DisposeAsync()
        {
            await Db.Connection.DisposeAsync();
        }

    }
    public static class WhyDoesThisHaveToExist
    {
        public static Task NonQuery(this DbConnection conn, string t)
        {
            DbCommand comm = conn.CreateCommand();
            comm.CommandText = t;
            return comm.ExecuteNonQueryAsync();
        }
    }
}