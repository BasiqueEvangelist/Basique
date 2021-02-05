namespace Basique.Modeling
{
    public class SqlGenerationSettings
    {
        public static readonly SqlGenerationSettings Sqlite = new(
            LastIdMethod.LastInsertRowId,
            ConcatFunction.DoublePipe
        );
        public static readonly SqlGenerationSettings MySql = new(
            LastIdMethod.LastInsertId,
            ConcatFunction.ConcatFunc
        );

        public LastIdMethod LastId { get; }
        public ConcatFunction Concat { get; }

        public SqlGenerationSettings(LastIdMethod lastId, ConcatFunction concat)
        {
            LastId = lastId;
            Concat = concat;
        }

        public enum LastIdMethod
        {
            LastInsertRowId,
            LastInsertId
        }

        public enum ConcatFunction
        {
            ConcatFunc,
            DoublePipe
        }
    }
}