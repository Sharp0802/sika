using System.ComponentModel.DataAnnotations;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class TemplateModel : IValidatableObject
{
    public TemplateModel(Project project, PostFrontMatter metadata, PostTree tree, string html)
    {
        Layout   = metadata.Layout;
        Metadata = metadata;
        Header   = new HeaderInfo(metadata.Title, metadata.Topic, metadata.Timestamps);
        Profile  = project.Profile;
        Contacts = project.Contacts;
        PostTree = tree;
        Html     = html;
    }

    [Required] public string Layout { get; }

    [Required] public PostFrontMatter Metadata { get; }

    [Required] public HeaderInfo Header { get; }

    [Required] public ProfileInfo Profile { get; }

    [Required] public Contacts Contacts { get; }

    [Required] public PostTree PostTree { get; }

    [Required(AllowEmptyStrings = true)] public string Html { get; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Layout), typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Header), typeof(PostFrontMatter)));
        list.AddRange(this.ValidateProperty(nameof(Profile), typeof(ProfileInfo)));
        list.AddRange(this.ValidateProperty(nameof(Contacts), typeof(Contacts)));
        list.AddRange(this.ValidateProperty(nameof(Html), typeof(PostTree)));
        return list;
    }
}