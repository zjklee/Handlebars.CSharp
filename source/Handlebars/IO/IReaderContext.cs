namespace HandlebarsDotNet
{
    public interface IReaderContext
    {
        int LineNumber { get; set; }
        int CharNumber { get; set; }
    }
}