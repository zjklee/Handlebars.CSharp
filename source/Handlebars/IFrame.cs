using System;
using HandlebarsDotNet.ValueProviders;

namespace HandlebarsDotNet
{
    /// <summary>
    /// Represents execution context
    /// </summary>
    public interface IFrame : IDisposable
    {
        BlockParamsValues BlockParams { get; }
        DataValues Data { get; }
        object Value { get; set; }
    }
}