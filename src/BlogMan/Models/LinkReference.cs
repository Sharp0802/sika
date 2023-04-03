using System.ComponentModel.DataAnnotations;
using BlogMan.Components;
using YamlDotNet.Serialization;

namespace BlogMan.Models;

[Serializable, YamlSerializable]
public partial class LinkReference : IValidatableObject
{
    [Required]
    public string? HRef { get; set; }
    
    [Required]
    public string? Name { get; set; }

    public LinkReference(string href, string name)
    {
        HRef = href;
        Name = name;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(HRef)));
        list.AddRange(this.ValidateProperty(nameof(Name)));
        return list;
    }
}