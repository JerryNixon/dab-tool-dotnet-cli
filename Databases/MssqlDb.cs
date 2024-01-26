using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

public class MssqlDb : IDb
{
    private readonly string connectionString;

    public MssqlDb(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public bool TryReadSql(out string sql, ref string[] errors)
    {
        try
        {
            var root = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
            var path = Path.Combine(root, $"Text/mssql.sql");
            sql = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            sql = default!;
            errors = new[] { $"{ex.GetType()}: {ex.Message}" };
        }

        return errors?.Length == 0;
    }

    public bool TryExecuteSql(DbConnection connection, string sql, ref DbDataReader reader, ref string[] errors)
    {
        try
        {
            if (connection?.State != ConnectionState.Open)
            {
                if (!TryOpen(ref connection!, ref errors))
                {
                    return false;
                }
            }

            var cmd = new SqlCommand(sql, connection as SqlConnection);
            reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            errors = new[] { $"{ex.GetType()}: {ex.Message}" };
        }

        return errors?.Length == 0;
    }

    public bool TryOpen(ref DbConnection connection, ref string[] errors)
    {
        try
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        catch (Exception ex)
        {
            errors = new[] { $"{ex.GetType()}: {ex.Message}" };
        }

        return errors?.Length == 0;
    }
}