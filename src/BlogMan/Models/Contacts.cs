using System.ComponentModel.DataAnnotations;
using BlogMan.Components;
using YamlDotNet.Serialization;

namespace BlogMan.Models;

[Serializable, YamlSerializable]
public partial class Contacts : IValidatableObject
{
    [Required]
    public string GitHub { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public LinkReference[] Misc { get; set; }

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