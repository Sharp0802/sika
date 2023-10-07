//     Copyright (C) 2023  Yeong-won Seo
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

using System.Diagnostics.Contracts;

namespace SIKA.Models.Utilities;

public static class UriUtils
{
    private const string Usable = "ABCDEFGHIJKLMNOPQRSTUVUXYZabcdefghijklmnopqrstuvwxyz0123456789-_.~";
    
    [Pure]
    public static string Normalize(string uri, char replace = '_')
    {
        return new string(uri.Normalize().Select(ch => Usable.Contains(ch) ? ch : replace).ToArray());
    }
}