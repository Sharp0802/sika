using sika.core.Text.Abstract;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace sika.core.Text;

public class Yaml : ITextFormat
{
    private static readonly ISerializer Serializer =
        new SerializerBuilder().WithNamingConvention(NullNamingConvention.Instance).Build();

    private static readonly IDeserializer Deserializer =
        new DeserializerBuilder().WithNamingConvention(NullNamingConvention.Instance).Build();

    public string Serialize<T>(T data) where T : class
    {
        return Serializer.Serialize(data, typeof(T));
    }

    public T? Deserialize<T>(string data) where T : class
    {
        return Deserializer.Deserialize<T>(data);
    }
}