using System.Text.Json;
using sika.core.Text.Abstract;

namespace sika.core.Text;

public class Json : ITextFormat
{
    public string Serialize<T>(T data) where T : class
    {
        return JsonSerializer.Serialize(data);
    }

    public T? Deserialize<T>(string data) where T : class
    {
        return JsonSerializer.Deserialize<T>(data);
    }
}