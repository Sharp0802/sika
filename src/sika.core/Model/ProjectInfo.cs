// Copyright (C)  2023  Yeong-won Seo
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

using System.Text.Json.Serialization;
using sika.core.Model.Abstract;

namespace sika.core.Model;

[method: JsonConstructor]
public class ProjectInfo(
    string rootUri,
    string postDirectory,
    string layoutDirectory,
    string siteDirectory) : ModelBase
{
    public string RootUri         { get; set; } = rootUri;
    public string PostDirectory   { get; set; } = postDirectory;
    public string LayoutDirectory { get; set; } = layoutDirectory;
    public string SiteDirectory   { get; set; } = siteDirectory;
}