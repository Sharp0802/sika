using System.ComponentModel.DataAnnotations;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class TemplateModel : IValidatableObject
{
    [Required]
    public string Layout { get; }
    
    [Required]
    public PostFrontMatter Metadata { get; }
    
    [Required]
    public HeaderInfo Header { get; }
    
    [Required]
    public ProfileInfo Profile { get; }
    
    [Required]
    public Contacts Contacts { get; }
    
    [Required]
    public PostTree PostTree { get; }
    
    [Required(AllowEmptyStrings = true)]
    public string Html { get; }

    public TemplateModel(Project project, PostFrontMatter metadata, PostTree tree, string html)
    {
        Layout = metadata.Layout;
        Metadata = metadata;
        Header = new HeaderInfo(metadata.Title, metadata.Topic, metadata.Timestamps);
        Profile = project.Profile;
        Contacts = project.Contacts;
        PostTree = tree;
        Html = html;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Layout)));
        list.AddRange(this.ValidateProperty(nameof(Header)));
        list.AddRange(this.ValidateProperty(nameof(Profile)));
        list.AddRange(this.ValidateProperty(nameof(Contacts)));
        list.AddRange(this.ValidateProperty(nameof(Html)));
        return list;
    }
}