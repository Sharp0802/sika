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