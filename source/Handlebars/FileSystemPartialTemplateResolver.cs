﻿using System;

namespace HandlebarsDotNet
{
    /// <inheritdoc />
    public class FileSystemPartialTemplateResolver : IPartialTemplateResolver
    {
        /// <inheritdoc />
        public bool TryRegisterPartial(IHandlebars env, string partialName, string templatePath)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            var handlebarsTemplateRegistrations = env.Configuration as IHandlebarsTemplateRegistrations ?? env.As<ICompiledHandlebars>().CompiledConfiguration;
            if (handlebarsTemplateRegistrations?.FileSystem == null || templatePath == null || partialName == null)
            {
                return false;
            }

            var partialPath = handlebarsTemplateRegistrations.FileSystem.Closest(templatePath,
                "partials/" + partialName + ".hbs");

            if (partialPath != null)
            {
                var compiled = env
                    .CompileView(partialPath);

                handlebarsTemplateRegistrations.RegisteredTemplates.Add(partialName, (writer, o) =>
                {
                    writer.Write(compiled(o));
                });

                return true;
            }

            // Failed to find partial in filesystem
            return false;
        }
    }
}
