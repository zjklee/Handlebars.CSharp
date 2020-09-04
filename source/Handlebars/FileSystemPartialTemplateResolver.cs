using System;
using System.IO;
using HandlebarsDotNet.Compiler;

namespace HandlebarsDotNet
{
    /// <inheritdoc />
    public class FileSystemPartialTemplateResolver : IPartialTemplateResolver
    {
        public bool TryResolvePartial(ICompiledHandlebarsConfiguration configuration, string partialName, out Action<TextWriter, object> partial)
        {
            var templatePath = configuration.TemplateProperties.TemplatePath;
            if (configuration.FileSystem == null || templatePath == null || partialName == null)
            {
                partial = null;
                return false;
            }

            var partialPath = configuration.FileSystem.Closest(templatePath, "partials/" + partialName + ".hbs");

            if (partialPath != null)
            {
                partial = HandlebarsCompiler.CompileView((handlebarsConfiguration, path) =>
                {
                    var fs = configuration.FileSystem;
                    if (fs == null) throw new InvalidOperationException("Cannot compile view when configuration.FileSystem is not set");
                    
                    var template = fs.GetFileContent(path);
                    if (template == null) throw new InvalidOperationException("Cannot find template at '" + path + "'");

                    return new StringReader(template);
                }, configuration);
                
                return true;
            }

            // Failed to find partial in filesystem
            partial = null;
            return false;
        }
    }
}