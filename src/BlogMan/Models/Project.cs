using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable, XmlRoot]
public partial class Project : IValidatableObject
{
    [Required]
    public ProjectInfo Info { get; set; }

    [Required]
    public ProfileInfo Profile { get; set; }
    
    [Required]
    public Contacts Contacts { get; set; }

    public Project(ProjectInfo info, ProfileInfo profile, Contacts contacts)
    {
        Info = info;
        Profile = profile;
        Contacts = contacts;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Info)));
        list.AddRange(this.ValidateProperty(nameof(Profile)));
        list.AddRange(this.ValidateProperty(nameof(Contacts)));
        return list;
    }
}