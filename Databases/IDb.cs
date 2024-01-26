using System.Data.Common;

public interface IDb
{
    public bool TryReadSql(out string sql, ref string[] errors);
    bool TryOpen(ref DbConnection connection, ref string[] errors);
    bool TryExecuteSql(DbConnection connection, string sql, ref DbDataReader reader, ref string[] errors);
}