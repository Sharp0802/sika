namespace sika.core.Components.Abstract;

public interface ILinker
{
    public Task CompileAsync(PageTree tree);
}