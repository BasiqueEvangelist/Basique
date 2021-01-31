using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LinqToDB;

namespace Basique.Benchmark
{
    public partial class Benchmarks
    {
        [Benchmark]
        public async Task<TestObject[]> Linq2DbToArray()
        {
            await using var db = new TestL2DBContext(l2dbBuilder.Build());
            return await db.TestObjects.ToArrayAsync();
        }
    }
}