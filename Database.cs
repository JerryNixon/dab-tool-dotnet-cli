using System.Data.Common;

public class Database
{
    private readonly IDb db;

    public Database(DatabaseType dbtype, string connectionstring)
    {
        if (dbtype == DatabaseType.mssql)
        {
            db = new MssqlDb(connectionstring);
        }
        else
        {
            throw new NotImplementedException(dbtype.ToString());
        }
    }

    public bool TryGetDbInfo(out Dictionary<string, object?>[] value, ref string[] errors)
    {
        value = default!;

        DbConnection connection = default!;

        if (!db.TryOpen(ref connection, ref errors))
        {
            return false;
        }

        if (!db.TryReadSql(out var sql, ref errors))
        {
            return false;
        }

        DbDataReader reader = default!;

        if (!db.TryExecuteSql(connection, sql, ref reader, ref errors))
        {
            return false;
        }

        value = TranslateReader(reader);

        return errors?.Length == 0;
    }


    private static Dictionary<string, object?>[] TranslateReader(DbDataReader reader)
    {
        if (reader is null || reader.IsClosed)
        {
            return Array.Empty<Dictionary<string, object?>>();
        }

        var data = new List<Dictionary<string, object?>>();

        while (reader.Read())
        {
            var rowData = Enumerable.Range(0, reader.FieldCount)
                .ToDictionary(i => reader.GetName(i), i => reader.IsDBNull(i) ? null : reader.GetValue(i));

            data.Add(rowData);
        }

        return data.ToArray();
    }
}