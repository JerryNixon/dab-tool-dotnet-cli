using static Writer;
using static Constants;
using System.Diagnostics;
using System.Text.Json;
using Dab.Tool.Utility;
using System.Text.RegularExpressions;

partial class Program
{
    private static string? readerJson = string.Empty;
    private static string envVarName = string.Empty;
    private static string connectionstring = string.Empty;
    private static string[] errors = Array.Empty<string>();

    public static void Main(string[] args)
    {
        if (args is null || !args.Any() || args.Exists(arg_h_variants))
        {
            WriteWarning("Help only.");
            WriteHelp();
            return;
        }

        if (!args.TryRead(out DatabaseType dbtype, arg_db_variants))
        {
            WriteWarning(arg_db_warning);
            dbtype = arg_db_default;
        }

        if (dbtype == DatabaseType.sample)
        {
            WriteWarning("Using sample data.");

            var root = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
            var path = Path.Combine(root, "Text/sample.json");
            readerJson = File.ReadAllText(path);
        }
        else
        {
            if (args.TryRead(out string envconn, arg_ce_variants))
            {
                if (envconn.TryGetEnvironmentVariable(out connectionstring, EnvironmentVariableTarget.Machine, EnvironmentVariableTarget.User))
                {
                    WriteWarning($"Found environment variable: {envconn} as {connectionstring}");
                }
                else
                {
                    WriteError(arg_ce_error);
                    WriteHelp();
                    return;
                }
            }

            if (string.IsNullOrEmpty(connectionstring) && !args.TryRead(out connectionstring, arg_c_variants))
            {
                WriteError(arg_c_error);
                WriteHelp();
                return;
            }

            connectionstring = Regex.Replace(connectionstring.Trim('"'), @"\\+", @"\");

            // write the connection string to the console
            if (args.Exists(arg_echo_variants))
            {
                WriteInfo($"Connection String: {connectionstring}");
            }

            if (args.TryRead(out envVarName, arg_e_variants))
            {
                WriteInfo(string.Format(arg_e_info, envVarName));
            }
            else
            {
                WriteWarning(arg_e_warning);
                envVarName = arg_e_default;
            }

            if (!args.Exists(arg_noe_variants))
            {
                // save connectionstring to environment file
                File.WriteAllLines(".ENV", new[] {
                    "# data-source.connection-string",
                    $"{envVarName}=\"{connectionstring}\""
                });
            }

            if (!new Database(dbtype, connectionstring).TryGetDbInfo(out var reader, ref errors))
            {
                WriteError(errors);
                return;
            }

            readerJson = JsonSerializer.Serialize(reader);

            if (readerJson is null)
            {
                WriteError("JsonSerializer.Serialize(reader) returned null.");
                return;
            }
        }

        if (!args.TryRead(out string output, arg_o_variants))
        {
            WriteError(arg_o_warning);
            output = arg_o_default;
        }

        if (!args.TryRead(out string template, arg_t_variants))
        {
            WriteError(arg_t_warning);
            template = arg_t_default;
        }

        var parameters = new (string, string)[]
        {
            ("readerJson", readerJson),
            ("environment", envVarName)
        };

        if (template?.Trim().ToLower() == "s")
        {
            GetScript(args, output, parameters);
        }
        else if (template?.Trim().ToLower() == "m")
        {
            GetModels(args, output, parameters);
        }
        else if (template?.Trim().ToLower() == "c")
        {
            GetScript(args, "dab-cli.cmd", parameters);
            GetModels(args, "models.cs", parameters);
            Process.Start("dab-cli.cmd").WaitForExit();
            Process.Start("notepad.exe", "dab-config.json");
            Process.Start("notepad.exe", ".ENV");
            WriteWarning("Starting...");
            Process.Start("dab", "start");
        }
        else
        {
            WriteError("Invalid template specified.");
            WriteHelp();
        }
    }

    private static void GetModels(string[] args, string output, (string, string)[] parameters)
    {
        var generator = new T4(template_references);

        var templates = new Templates(generator);

        if (templates.TryMakeModels(parameters, output!, ref errors) && !args.Exists(arg_non_variants))
        {
            Process.Start("notepad.exe", output!);
        }
        else
        {
            WriteError(errors);
        }
    }

    private static void GetScript(string[] args, string output, (string, string)[] parameters)
    {
        var generator = new T4(template_references);
        var templates = new Templates(generator);

        if (templates.TryMakeScript(parameters, output!, ref errors) && !args.Exists(arg_non_variants))
        {
            Process.Start("notepad.exe", output!);
        }
        else
        {
            WriteError(errors);
        }
    }
}