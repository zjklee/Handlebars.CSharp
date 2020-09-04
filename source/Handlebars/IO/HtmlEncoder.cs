using System.Globalization;
using System.Runtime.CompilerServices;

namespace HandlebarsDotNet
{
    /// <summary>
    /// <inheritdoc />
    /// Produces <c>HTML</c> safe output.
    /// </summary>
    public class HtmlEncoder : ITextEncoder
    {
        /// <inheritdoc />
        public string Encode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;


            // Detect if we need to allocate a stringbuilder and new string
            for (var i = 0; i < text.Length; i++)
            {
                if (RequireEncoding(text[i]))
                {
                    return ReallyEncode(text, i);
                }
            }

            return text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RequireEncoding(char c)
        {
            switch (c)
            {
                case '"':
                case '&':
                case '<':
                case '>':
                    return true;
                
                default:
                    return c > 159;
            }
        }

        private static string ReallyEncode(string text, int i)
        {
            using (var container = StringBuilderPool.Shared.Use())
            {
                var sb = container.Value;
                sb.Append(text, 0, i);
                for (; i < text.Length; i++)
                {
                    switch (text[i])
                    {
                        case '"':
                            sb.Append("&quot;");
                            break;
                        case '&':
                            sb.Append("&amp;");
                            break;
                        case '<':
                            sb.Append("&lt;");
                            break;
                        case '>':
                            sb.Append("&gt;");
                            break;

                        default:
                            if (text[i] > 159)
                            {
                                sb.Append("&#");
                                sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
                                sb.Append(";");
                            }
                            else
                                sb.Append(text[i]);

                            break;
                    }
                }

                return sb.ToString();
            }
        }
    }
}