using System;
using HerringORM;
using System.Linq;
// using Xunit;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using HerringORM.Modeling;
using System.Data.Common;
using System.Collections.Generic;

namespace HerringORM.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program().Works().GetAwaiter().GetResult();
        }
        class TestObject
        {
            public string Test;
            public int Value;

            public override string ToString() => $"(\"{Test}\";{Value})";
        }
        class TestContext : DatabaseContext
        {
            public Table<TestObject> TestObjects => new Table<TestObject>(this);

            public TestContext(DbConnection conn) : base(conn)
            {
                Table<TestObject>(build =>
                {
                    build.RemoteName("testobjects");
                });
            }
        }

        public async Task Works()
        {
            SqliteConnection conn = new SqliteConnection(new SqliteConnectionStringBuilder("")
            {
                DataSource = ":memory:"
            }.ToString());
            await conn.OpenAsync();

            await conn.NonQuery("CREATE TABLE testobjects (test TEXT, value INT);");

            TestContext tc = new TestContext(conn);
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 0, Test = "oof" });
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 1, Test = "foo" });
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 2, Test = "bar" });
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 3, Test = "baz" });
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 4, Test = "qux" });
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 5, Test = "quux" });

            List<TestObject> l = await tc.TestObjects
                .ToListAsync();

            Console.WriteLine(string.Join('\n', l));

            List<TestObject> lwhere = await tc.TestObjects
                .Where(x => (x.Value > 4 ? true : x.Value < 3) && (x.Value > 1 ^ x.Value == 0))
                .ToListAsync();

            Console.WriteLine(string.Join('\n', lwhere));
        }
    }
    public static class why
    {
        public static Task NonQuery(this DbConnection conn, string t)
        {
            DbCommand comm = conn.CreateCommand();
            comm.CommandText = t;
            return comm.ExecuteNonQueryAsync();
        }
    }
}
