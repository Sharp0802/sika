using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class LinkReference : IValidatableObject
{
    [JsonConstructor]
    public LinkReference(string href, string name)
    {
        HRef = href;
        Name = name;
    }

    [Required] public string HRef { get; set; }

    [Required] public string Name { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(HRef), HRef.GetType()));
        list.AddRange(this.ValidateProperty(nameof(Name), Name.GetType()));
        return list;
    }
}