using System;
using System.IO;
using System.Runtime.CompilerServices;

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
            writer.Write(SafeString.Create(value));
        }

        /// <summary>
        /// Writes an encoded string using <see cref="ITextEncoder"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteSafeString(this TextWriter writer, object value)
        {
            switch (value)
            {
                case null:
                    return;
                    
                case string v when string.IsNullOrEmpty(v):
                    return;
                
                case string v:
                    writer.Write(SafeString.Create(v));
                    return;

                default:
                    writer.Write(SafeString.Create(value.ToString()));
                    return;
            }
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
    public interface ISafeString : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        string Value { get; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SafeString : ISafeString
    {
        private static readonly InternalObjectPool<SafeString> Pool = new InternalObjectPool<SafeString>(new Policy());
        
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SafeString Create(string value)
        {
            var safeString = Pool.Get();
            safeString.Value = value;
            return safeString;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private SafeString()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value;
        
        public void Dispose() => Pool.Return(this);
        
        private class Policy : IInternalObjectPoolPolicy<SafeString>
        {
            public SafeString Create() => new SafeString();

            public bool Return(SafeString item)
            {
                //item.Value = null;
                return true;
            }
        }
    }
}

