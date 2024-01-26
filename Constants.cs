using MySql.Data.MySqlClient;

using Npgsql;

using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.Json;

public static class Constants
{
    public static readonly string[] arg_h_variants = new[] { "-?", "-h", "--help" };
    public static readonly string[] arg_c_variants = new[] { "-c", "--connectionstring" };
    public static readonly string[] arg_ce_variants = new[] { "-ce" };
    public static readonly string[] arg_db_variants = new[] { "-db", "--databasetype" };
    public static readonly string[] arg_e_variants = new[] { "-e", "--environmentvariable" };
    public static readonly string[] arg_noe_variants = new[] { "-noe", "--noenvironment" };
    public static readonly string[] arg_o_variants = new[] { "-o", "--outputfile" };
    public static readonly string[] arg_non_variants = new[] { "-non", "--nonotepad" };
    public static readonly string[] arg_t_variants = new[] { "-t", "--template" };
    public static readonly string[] arg_echo_variants = new[] { "--echo" };

    public const string arg_c_error = "Arg (-c) not suppied. This is required.";
    public const string arg_ce_error = "Arg (-ce) suppied. Value not found.";

    public const string arg_db_warning = "Arg (-db) not supplied. Default is database type is 'mssql'.";
    public const string arg_e_info = "Arg (-e) supplied with {-0}. Connection String will be stored in this environment variable.";
    public const string arg_e_warning = "Arg (-e) not supplied. Connection String will be stored in default environment variable 'my-connection-string'.";
    public const string arg_o_warning = "Arg (-o) not suppied. Default output file is 'out.txt'.";
    public const string arg_t_warning = "Arg (-t) not supplied. Default template is '(s)cript'.";

    public const DatabaseType arg_db_default = DatabaseType.mssql;
    public const string arg_e_default = "my-connection-string";
    public const string arg_o_default = "out.txt";
    public const string arg_t_default = "s";

    public static readonly Type[] template_references = new[]
    {
        typeof(SqlConnection),
        typeof(MySqlConnection),
        typeof(NpgsqlConnection),
        typeof(DiagnosticSource),
        typeof(JsonSerializer),
        typeof(System.Text.Encodings.Web.HtmlEncoder)
    };

}
