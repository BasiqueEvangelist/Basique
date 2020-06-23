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
            await tc.TestObjects.CreateAsync(() => new TestObject() { Value = 1, Test = "beb" });

            List<TestObject> l = await tc.TestObjects
                .ToListAsync();

            Console.WriteLine(string.Join(' ', l));
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
