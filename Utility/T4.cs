using Mono.TextTemplating;

using System.CodeDom.Compiler;

namespace Dab.Tool.Utility
{
    public class T4
    {
        private readonly Type[] references = default!;

        public T4(params Type[] references)
        {
            this.references = references;
        }

        public bool Generate(string templatePath, string outputPath, out string[] errors, params (string Name, string Value)[] parameters)
        {
            var generator = new TemplateGenerator();

            generator.UseInProcessCompiler();

            AddReferences(generator);

            AddParameters(parameters, generator);

            Console.WriteLine($"Generating '{templatePath}' -> '{outputPath}'");

            var success = generator.ProcessTemplateAsync(templatePath, outputPath).Result;

            errors = GetErrors(generator);

            return success;

            void AddReferences(TemplateGenerator generator)
            {
                if (references is null)
                {
                    return;
                }

                foreach (var item in references)
                {
                    var path = item.Assembly.Location;
                    generator.Refs.Add(path);
                }
            }

            void AddParameters((string Name, string Value)[] parameters, TemplateGenerator generator)
            {
                foreach (var item in parameters)
                {
                    generator.AddParameter(null, null, item.Name, item.Value);
                }
            }

            string[] GetErrors(TemplateGenerator generator)
            {
                var Errors = new List<string>();
                foreach (CompilerError error in generator.Errors)
                {
                    Errors.Add(error.ToString());
                }
                return Errors.ToArray();
            }
        }
    }
}
