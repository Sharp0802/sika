namespace sika.core.Text.Abstract;

public interface ITextFormat
{
    public string Serialize<T>(T data) where T : class;

    public T? Deserialize<T>(string data) where T : class;
}