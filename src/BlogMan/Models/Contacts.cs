using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class Contacts : IValidatableObject
{
    [JsonConstructor]
    public Contacts(string github, string email, LinkReference[] misc)
    {
        GitHub = github;
        Email  = email;
        Misc   = misc;
    }

    [Required] public string GitHub { get; set; }

    [Required] public string Email { get; set; }

    [Required] public LinkReference[] Misc { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(GitHub), typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Email),  typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Misc),   typeof(LinkReference[])));
        return list;
    }
}