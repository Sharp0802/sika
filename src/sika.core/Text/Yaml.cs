// Copyright (C)  2024  Yeong-won Seo
// 
// SIKA is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SIKA is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using sika.core.Text.Abstract;
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