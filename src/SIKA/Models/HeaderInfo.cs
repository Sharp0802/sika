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
public partial class HeaderInfo : IValidatableObject
{
    [JsonConstructor]
    public HeaderInfo(string title, string[] topics, DateTime[] timestamps)
    {
        Title      = title;
        Topics     = topics;
        Timestamps = timestamps;
    }

    [Required(AllowEmptyStrings = false)] public string Title { get; set; }

    [Required] public string[] Topics { get; set; }

    [Required] public DateTime[] Timestamps { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Title),      typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Topics),     typeof(string[])));
        list.AddRange(this.ValidateProperty(nameof(Timestamps), typeof(DateTime[])));
        return list;
    }
}