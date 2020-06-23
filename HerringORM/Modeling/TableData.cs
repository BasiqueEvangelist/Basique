using System.Data.Common;
namespace HerringORM.Modeling
{
    internal class TableData
    {
        public string Name;
    }
    public class TableBuilder<T>
    {
        private readonly TableData data;

        internal TableBuilder(TableData data)
        {
            this.data = data;
        }

        public void RemoteName(string name) => data.Name = name;
    }
}