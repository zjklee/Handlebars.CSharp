using System;
using System.IO;

namespace HandlebarsDotNet
{
    /// <summary>
    /// Template resolver that gets called when an unknown partial is requested.
    /// </summary>
    public interface IPartialTemplateResolver
    {
        bool TryResolvePartial(ICompiledHandlebarsConfiguration configuration, string partialName, out Action<TextWriter, object> partial);
    }
}
