using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIKA.Components;

namespace SIKA.Models;

[Serializable]
public partial class ProjectInfo : IValidatableObject
{
    [JsonConstructor]
    public ProjectInfo(
        string  name,
        Version apiVersion,
        string  rootUri,
        string  postDirectory,
        string  layoutDirectory,
        string  siteDirectory,
        string  buildDirectory)
    {
        Name            = name;
        ApiVersion      = apiVersion;
        RootUri         = rootUri;
        PostDirectory   = postDirectory;
        LayoutDirectory = layoutDirectory;
        SiteDirectory   = siteDirectory;
        BuildDirectory  = buildDirectory;
    }

    [Required] public string Name { get; set; }

    [Required] public Version ApiVersion { get; set; }

    [Required] public string RootUri { get; set; }

    [Required] public string PostDirectory { get; set; }

    [Required] public string LayoutDirectory { get; set; }

    [Required] public string SiteDirectory { get; set; }

    [Required] public string BuildDirectory { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Name),            typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(ApiVersion),      typeof(Version)));
        list.AddRange(this.ValidateProperty(nameof(RootUri),         typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(PostDirectory),   typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(LayoutDirectory), typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(SiteDirectory),   typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(BuildDirectory),  typeof(string)));
        return list;
    }
}