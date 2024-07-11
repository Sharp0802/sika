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
public class Project(ProjectInfo info, ProfileInfo profile, Contacts contacts) : ModelBase
{
    public ProjectInfo Info     { get; set; } = info;
    public ProfileInfo Profile  { get; set; } = profile;
    public Contacts    Contacts { get; set; } = contacts;

    public void InitializeDirectories(string dir)
    {
        Info.LayoutDirectory = Path.Combine(dir, Info.LayoutDirectory);
        Info.PostDirectory   = Path.Combine(dir, Info.PostDirectory);
        Info.SiteDirectory   = Path.Combine(dir, Info.SiteDirectory);
    }
}