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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIKA.Components;

namespace SIKA.Models;

[Serializable]
public partial class ProfileInfo : IValidatableObject
{
    [JsonConstructor]
    public ProfileInfo(string userName, string profileImage)
    {
        UserName     = userName;
        ProfileImage = profileImage;
    }

    [Required] public string UserName { get; set; }

    [Required] public string ProfileImage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(UserName),     typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(ProfileImage), typeof(string)));
        return list;
    }
}