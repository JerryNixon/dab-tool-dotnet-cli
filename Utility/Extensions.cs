public static class Extensions
{
    public static bool TryGetEnvironmentVariable(this string name, out string value, params EnvironmentVariableTarget[] targets)
    {
        foreach (var target in targets)
        {
            value = Environment.GetEnvironmentVariable(name, target)!;
            if (!string.IsNullOrEmpty(value))
            {
                return true;
            }
        }

        value = default!;
        return false;
    }

    public static bool Exists(this string[] args, params string[] names)
    {
        return args.Any(x => names.Contains(x) || names.Contains(x.ToLower()));
    }

    public static bool TryRead(this string[] args, out string value, params string[] names)
    {
        value = null!;

        foreach (var name in names)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = args.SkipWhile(arg => arg != name).Skip(1).FirstOrDefault() ?? string.Empty;
            }
        }
        return !string.IsNullOrEmpty(value);
    }

    public static bool TryRead(this string[] args, out DatabaseType type, params string[] names)
    {
        var value = string.Empty;

        foreach (var name in names)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = args.SkipWhile(arg => arg != name).Skip(1).FirstOrDefault() ?? string.Empty;
            }
        }

        return Enum.TryParse(value, true, out type);
    }
}