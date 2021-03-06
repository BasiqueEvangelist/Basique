using System.Text;
using System.Security.Cryptography;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Basique.Modeling;
using Microsoft.Data.Sqlite;
using Xunit;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class TestEnvironment : IAsyncLifetime
    {
        public class TestObject
        {
            public int Id;
            public string Test;
            public int Value;
            public int? NullableValue;


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

        public class TestJoin
        {
            public TestObject First { get; set; }
            public TestObject Second { get; set; }
        }

        public class TestContext : BasiqueSchema
        {
            public Table<TestObject> TestObjects => new(this);
            public View<TestJoin> TestJoin => new(this);

            public TestContext(Func<DbConnection> conn, SqlGenerationSettings sqlGen) : base(conn, sqlGen)
            {
                Table<TestObject>(build =>
                {
                    build.RemoteName("testobjects");

                    build.Field(testobjects => testobjects.Id)
                        .Id();
                    build.Field(testobjects => testobjects.Test);
                    build.Field(testobjects => testobjects.Value);
                    build.Field(testobjects => testobjects.NullableValue);
                });

                Table<TestJoin>(build =>
                {
                    build.RemoteName("v_testjoins");

                    build.Field(v_testjoins => v_testjoins.First.Test)
                        .RemoteName("first_test");
                    build.Field(v_testjoins => v_testjoins.Second.Test)
                        .RemoteName("second_test");
                    build.Field(v_testjoins => v_testjoins.First.Value)
                        .RemoteName("first_value");
                    build.Field(v_testjoins => v_testjoins.Second.Value)
                        .RemoteName("second_value");
                });
            }
        }

        protected TestContext Db;
        protected SqliteConnection connection;
        protected ITestOutputHelper output;

        public TestEnvironment(ITestOutputHelper output)
        {
            this.output = output;
        }

        public async Task InitializeAsync()
        {
            string uid;
            {
                var type = output.GetType();
                var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
                var test = (ITest)testMember.GetValue(output);
                var hash = new SHA1Managed();
                uid = Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(test.DisplayName)));
            }
            string connString = new SqliteConnectionStringBuilder()
            {
                Mode = SqliteOpenMode.Memory,
                Cache = SqliteCacheMode.Shared,
                DataSource = "file:" + uid
            }.ToString();
            connection = new SqliteConnection(connString);
            await connection.OpenAsync();

            Db = new TestContext(() => new SqliteConnection(connString), SqlGenerationSettings.Sqlite)
            {
                Logger = new XunitLogger(output)
            };

            await using (var trans = await Db.MintTransaction())
            {
                await trans.NonQuery("CREATE TABLE testobjects (id INTEGER PRIMARY KEY, test TEXT, value INT, nullablevalue INT);");
                await trans.NonQuery("CREATE VIEW v_testjoins AS SELECT first.test first_test, second.test second_test, first.value first_value, second.value second_value FROM testobjects first JOIN testobjects second ON first.value = second.value;");

                await trans.Commit();
            }

            await Db.TestObjects.Create(() => new TestObject() { Value = 0, Test = "oof" }).VoidAsync();
            await Db.TestObjects.Create(() => new TestObject() { Value = 1, Test = "foo" }).VoidAsync();
            await Db.TestObjects.Create(() => new TestObject() { Value = 2, Test = "bar" }).VoidAsync();
            await Db.TestObjects.Create(() => new TestObject() { Value = 3, Test = "baz" }).VoidAsync();
            await Db.TestObjects.Create(() => new TestObject() { Value = 4, Test = "qux" }).VoidAsync();
            await Db.TestObjects.Create(() => new TestObject() { Value = 5, Test = "quux" }).VoidAsync();
        }

        public async Task DisposeAsync()
        {
            await connection.DisposeAsync();
        }
    }
}