namespace Basique.Modeling
{
    public class SqlGenerationSettings
    {
        public static readonly SqlGenerationSettings Sqlite = new(LastIdMethod.LastInsertRowId);
        public static readonly SqlGenerationSettings MySql = new(LastIdMethod.LastInsertId);

        public LastIdMethod LastId { get; }

        public SqlGenerationSettings(LastIdMethod lastId)
        {
            LastId = lastId;
        }

        public enum LastIdMethod
        {
            LastInsertRowId,
            LastInsertId
        }
    }
}