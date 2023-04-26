using System.ComponentModel.DataAnnotations;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class Project : IValidatableObject
{
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
        list.AddRange(this.ValidateProperty(nameof(Info)));
        list.AddRange(this.ValidateProperty(nameof(Profile)));
        list.AddRange(this.ValidateProperty(nameof(Contacts)));
        return list;
    }
}