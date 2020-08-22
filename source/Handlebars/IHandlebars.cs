using System;
using System.IO;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.Helpers.BlockHelpers;

namespace HandlebarsDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHandlebars
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        Action<TextWriter, object> Compile(TextReader template);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        Func<object, string> Compile(string template);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        Func<object, string> CompileView(string templatePath);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="readerFactoryFactory">Can be null</param>
        /// <returns></returns>
        Action<TextWriter, object> CompileView(string templatePath, ViewReaderFactory readerFactoryFactory);
        
        /// <summary>
        /// 
        /// </summary>
        HandlebarsConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="template"></param>
        void RegisterTemplate(string templateName, Action<TextWriter, object> template);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="template"></param>
        void RegisterTemplate(string templateName, string template);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperName"></param>
        /// <param name="helperFunction"></param>
        void RegisterHelper(string helperName, HandlebarsHelper helperFunction);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperName"></param>
        /// <param name="helperFunction"></param>
        void RegisterHelper(string helperName, HandlebarsReturnHelper helperFunction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperName"></param>
        /// <param name="helperFunction"></param>
        void RegisterHelper(string helperName, HandlebarsBlockHelper helperFunction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperName"></param>
        /// <param name="helperFunction"></param>
        void RegisterHelper(string helperName, HandlebarsReturnBlockHelper helperFunction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperObject"></param>
        void RegisterHelper(BlockHelperDescriptor helperObject);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperObject"></param>
        void RegisterHelper(HelperDescriptor helperObject);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperObject"></param>
        void RegisterHelper(ReturnBlockHelperDescriptor helperObject);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helperObject"></param>
        void RegisterHelper(ReturnHelperDescriptor helperObject);
    }
    
    internal interface ICompiledHandlebars
    {
        ICompiledHandlebarsConfiguration CompiledConfiguration { get; }
    }
}

