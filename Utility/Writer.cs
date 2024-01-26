public static class Writer
{
    public static void WriteInfo(params string[] errors) => ConsoleWriteLine(errors, ConsoleColor.White);

    public static void WriteError(params string[] errors) => ConsoleWriteLine(errors, ConsoleColor.Red);

    public static void WriteWarning(params string[] warnings) => ConsoleWriteLine(warnings, ConsoleColor.Yellow);

    public static void WriteHelp()
    {
        var root = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
        var path = Path.Combine(root, "Text/help.txt");
        ConsoleWriteLine(File.ReadAllText(path), ConsoleColor.White);
    }

    public static void ConsoleWriteLine(string text, ConsoleColor? foreground = null, ConsoleColor? background = null) => ConsoleWriteLine(new[] { text }, foreground, background);

    public static void ConsoleWriteLine(string[] text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        Console.ForegroundColor = foreground ?? Console.ForegroundColor;
        Console.BackgroundColor = background ?? Console.BackgroundColor;
        foreach (var item in text)
        {
            Console.WriteLine(item);
        }
        Console.ResetColor();
    }
}
