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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIKA.Components;

namespace SIKA.Models;

[Serializable]
public partial class Project : IValidatableObject
{
    [JsonConstructor]
    public Project(ProjectInfo info, ProfileInfo profile, Contacts contacts)
    {
        Info     = info;
        Profile  = profile;
        Contacts = contacts;
    }

    [Required] public ProjectInfo Info { get; set; }

    [Required] public ProfileInfo Profile { get; set; }

    [Required] public Contacts Contacts { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Info),     typeof(ProjectInfo)));
        list.AddRange(this.ValidateProperty(nameof(Profile),  typeof(ProfileInfo)));
        list.AddRange(this.ValidateProperty(nameof(Contacts), typeof(Contacts)));
        return list;
    }
}