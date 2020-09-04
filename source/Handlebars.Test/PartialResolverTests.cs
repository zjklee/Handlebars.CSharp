using System;
using System.IO;
using HandlebarsDotNet.Compiler;
using Xunit;

namespace HandlebarsDotNet.Test
{
    public class PartialResolverTests
    {
        public class CustomPartialResolver : IPartialTemplateResolver
        {
            public bool TryResolvePartial(ICompiledHandlebarsConfiguration configuration, string partialName, out Action<TextWriter, object> partial)
            {
                if (partialName == "person")
                {
                    partial = HandlebarsCompiler.Compile("{{name}}", configuration);
                    return true;
                }

                partial = null;
                return false;
            }
        }

        [Fact]
        public void BasicPartial()
        {
            string source = "Hello, {{>person}}!";

            var handlebars = Handlebars.Create(new HandlebarsConfiguration
            {
                PartialTemplateResolver = new CustomPartialResolver()
            });


            var template = handlebars.Compile(source);

            var data = new {
                name = "Marc"
            };
            
            var result = template(data);
            Assert.Equal("Hello, Marc!", result);
        }
    }
}

