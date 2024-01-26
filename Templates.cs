using Dab.Tool.Utility;

public class Templates
{
    private readonly T4 generator;

    public Templates(T4 generator)
    {
        this.generator = generator;
    }

    public bool TryMakeModels((string, string)[] parameters, string output, ref string[] errors)
    {
        try
        {
            var template = GetTemplatePath("Templates/Dab.Models.tt");
            return generator.Generate(template, output, out errors, parameters);
        }
        catch (Exception ex)
        {
            errors = new[] { ex.Message };
            return false;
        }
    }

    public bool TryMakeScript((string, string)[] parameters, string output, ref string[] errors)
    {
        try
        {
            var template = GetTemplatePath("Templates/Dab.Scripts.tt");
            return generator.Generate(template, output, out errors, parameters);
        }
        catch (Exception ex)
        {
            errors = new[] { $"{ex.GetType()}: {ex.Message}" };
            return false;
        }
    }

    private string GetTemplatePath(string name)
    {
        var path = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
        var template = Path.Combine(path, name);
        template = Path.GetFullPath(template);

        if (!File.Exists(template))
        {
            throw new FileNotFoundException(template);
        }

        return template;
    }
}