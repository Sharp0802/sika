using System.ComponentModel.DataAnnotations;
using BlogMan.Components;
using YamlDotNet.Serialization;

namespace BlogMan.Models;

[Serializable, YamlSerializable]
public partial class Contacts : IValidatableObject
{
    [Required(AllowEmptyStrings = false)]
    public string GitHub { get; }
    
    [Required(AllowEmptyStrings = false)]
    public string Email { get; }
    
    [Required]
    public LinkReference[] Misc { get; }

    public Contacts(string github, string email, LinkReference[] misc)
    {
        GitHub = github;
        Email = email;
        Misc = misc;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(GitHub)));
        list.AddRange(this.ValidateProperty(nameof(Email)));
        list.AddRange(this.ValidateProperty(nameof(Misc)));
        return list;
    }
}