using System.ComponentModel.DataAnnotations;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class ProjectInfo : IValidatableObject
{
    public ProjectInfo(string name, Version version, string post, string layout, string site, string build)
    {
        Name            = name;
        ApiVersion      = version;
        PostDirectory   = post;
        LayoutDirectory = layout;
        SiteDirectory   = site;
        BuildDirectory  = build;
    }

    [Required] public string Name { get; set; }

    [Required] public Version ApiVersion { get; set; }

    [Required] public string PostDirectory { get; set; }

    [Required] public string LayoutDirectory { get; set; }

    [Required] public string SiteDirectory { get; set; }

    [Required] public string BuildDirectory { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Name)));
        list.AddRange(this.ValidateProperty(nameof(ApiVersion)));
        list.AddRange(this.ValidateProperty(nameof(PostDirectory)));
        list.AddRange(this.ValidateProperty(nameof(LayoutDirectory)));
        list.AddRange(this.ValidateProperty(nameof(SiteDirectory)));
        list.AddRange(this.ValidateProperty(nameof(BuildDirectory)));
        return list;
    }
}