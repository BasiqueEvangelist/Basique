using System;
using System.Data.Common;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Basique.Modeling;
using BenchmarkDotNet.Attributes;
using Microsoft.Data.Sqlite;

namespace Basique.Benchmark
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

    public class TestContext : BasiqueSchema
    {
        public Table<TestObject> TestObjects => new(this);

        public TestContext(Func<DbConnection> conn, SqlGenerationSettings sqlGen) : base(conn, sqlGen)
        {
            Table<TestObject>(build =>
            {
                build.RemoteName("testobjects");

                build.Field(testobjects => testobjects.Test);
                build.Field(testobjects => testobjects.Value);
            });
        }
    }


    public class BenchmarkEnvironment
    {
        public TestContext schema;
        public SqliteConnection holdConn;

        public async Task GlobalSetup()
        {
            byte[] data = new byte[64];
            RandomNumberGenerator.Create().GetNonZeroBytes(data);
            string connString = new SqliteConnectionStringBuilder()
            {
                Mode = SqliteOpenMode.Memory,
                Cache = SqliteCacheMode.Shared,
                DataSource = "file:" + Convert.ToBase64String(data)
            }.ToString();

            holdConn = new SqliteConnection(connString);
            await holdConn.OpenAsync();

            schema = new TestContext(() => new SqliteConnection(connString), SqlGenerationSettings.Sqlite);

            await using var trans = await schema.MintTransaction();
            await trans.NonQuery("CREATE TABLE testobjects (id INTEGER PRIMARY KEY, test TEXT, value INT, nullablevalue INT);");
            await trans.Commit();
        }

        public async Task GlobalCleanup()
        {
            await holdConn.DisposeAsync();
        }
    }
}