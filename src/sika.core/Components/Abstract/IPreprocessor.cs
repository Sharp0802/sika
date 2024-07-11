namespace sika.core.Components.Abstract;

public interface IPreprocessor
{
    public Task<PageLeafData> PreprocessAsync(FileInfo file);
}