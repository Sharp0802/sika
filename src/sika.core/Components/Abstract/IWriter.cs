namespace sika.core.Components.Abstract;

public interface IWriter
{
    public Task WriteAsync(PageTree tree);
}