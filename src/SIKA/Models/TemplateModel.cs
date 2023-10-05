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
using SIKA.Components;
using SIKA.Models.Posts;

namespace SIKA.Models;

[Serializable]
public partial class TemplateModel : IValidatableObject
{
    public TemplateModel(Project project, PostFrontMatter metadata, PostRoot tree, string html)
    {
        Layout   = metadata.Layout;
        Metadata = metadata;
        Header   = new HeaderInfo(metadata.Title, metadata.Topic, metadata.Timestamps);
        Profile  = project.Profile;
        Contacts = project.Contacts;
        PostTree = tree;
        Html     = html;
    }

    [Required] public string Layout { get; }

    [Required] public PostFrontMatter Metadata { get; }

    [Required] public HeaderInfo Header { get; }

    [Required] public ProfileInfo Profile { get; }

    [Required] public Contacts Contacts { get; }

    [Required] public PostRoot PostTree { get; }

    [Required(AllowEmptyStrings = true)] public string Html { get; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Layout),   typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Header),   typeof(PostFrontMatter)));
        list.AddRange(this.ValidateProperty(nameof(Profile),  typeof(ProfileInfo)));
        list.AddRange(this.ValidateProperty(nameof(Contacts), typeof(Contacts)));
        list.AddRange(this.ValidateProperty(nameof(Html),     typeof(PostTree)));
        return list;
    }
}