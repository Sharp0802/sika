using YamlDotNet.Serialization;

namespace SIKA.Components;

public static class Yaml
{
    [ThreadStatic] private static ISerializer?   _sIO;
    [ThreadStatic] private static IDeserializer? _dIO;

    private static ISerializer Serializer => _sIO ??= new SerializerBuilder().Build();

    private static IDeserializer Deserializer => _dIO ??= new DeserializerBuilder().Build();

    public static void Serialize<T>(TextWriter writer, T obj)
    {
        Serializer.Serialize(writer, obj, typeof(T));
    }

    public static T Deserialize<T>(TextReader reader)
    {
        return Deserializer.Deserialize<T>(reader);
    }
}