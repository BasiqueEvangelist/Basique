using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Basique.Modeling;
using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Basique.Benchmark
{
    [MinColumn]
    [MaxColumn]
    public class Benchmarks : BenchmarkEnvironment
    {
        [GlobalSetup]
        public Task Startup() => GlobalSetup();
        [GlobalCleanup]
        public Task Cleanup() => GlobalCleanup();

        [Benchmark]
        public async Task<TestObject[]> BasiqueToArray() => await schema.TestObjects.ToArrayAsync();

        [Benchmark]
        public async Task<TestObject[]> SqlReaderToArray()
        {
            await using var conn = await schema.MintConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM testobjects";
            await using var reader = await cmd.ExecuteReaderAsync();
            List<TestObject> objects = new();
            while (await reader.ReadAsync())
            {
                TestObject obj = new();
                obj.Test = reader.GetString(reader.GetOrdinal("test"));
                obj.Value = reader.GetInt32(reader.GetOrdinal("value"));
                objects.Add(obj);
            }
            return objects.ToArray();
        }

        [Benchmark]
        public async Task<TestObject[]> DapperToArray()
        {
            await using var conn = await schema.MintConnection();
            return (await conn.QueryAsync<TestObject>("SELECT * FROM testobjects")).ToArray();
        }
    }
}
