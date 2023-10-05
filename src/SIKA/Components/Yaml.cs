//    Copyright (C) 2023  Yeong-won Seo
// 
//     SIKA is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     SIKA is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

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