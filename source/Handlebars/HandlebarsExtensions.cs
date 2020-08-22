using System;
using System.IO;

namespace HandlebarsDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public static class HandlebarsExtensions
    {
        /// <summary>
        /// Writes an encoded string using <see cref="ITextEncoder"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteSafeString(this TextWriter writer, string value)
        {
            writer.Write(new SafeString(value));
        }

        /// <summary>
        /// Writes an encoded string using <see cref="ITextEncoder"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteSafeString(this TextWriter writer, object value)
        {
            if (value is string str)
            {
                writer.Write(new SafeString(str));
                return;
            }
            
            writer.Write(new SafeString(value.ToString()));
        }
        
        /// <summary>
        /// Allows configuration manipulations
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HandlebarsConfiguration Configure(this HandlebarsConfiguration configuration, Action<HandlebarsConfiguration> config)
        {
            config(configuration);

            return configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static object This(this HelperOptions options, object context, Func<HelperOptions, Action<TextWriter, object>> selector)
        {
            using var writer = ReusableStringWriter.Get(options.Configuration.FormatProvider);
            selector(options)(writer, context);
            return writer.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISafeString
    {
        /// <summary>
        /// 
        /// </summary>
        string Value { get; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class SafeString : ISafeString
    {
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SafeString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }
    }
}

