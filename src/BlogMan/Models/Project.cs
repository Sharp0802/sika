using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class Project : IValidatableObject
{
    [JsonConstructor]
    public Project(ProjectInfo info, ProfileInfo profile, Contacts contacts)
    {
        Info     = info;
        Profile  = profile;
        Contacts = contacts;
    }

    [Required] public ProjectInfo Info { get; set; }

    [Required] public ProfileInfo Profile { get; set; }

    [Required] public Contacts Contacts { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Info),     typeof(ProjectInfo)));
        list.AddRange(this.ValidateProperty(nameof(Profile),  typeof(ProfileInfo)));
        list.AddRange(this.ValidateProperty(nameof(Contacts), typeof(Contacts)));
        return list;
    }
}