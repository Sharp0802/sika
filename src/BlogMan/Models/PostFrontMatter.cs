using System.ComponentModel.DataAnnotations;
using BlogMan.Components;
using YamlDotNet.Serialization;

namespace BlogMan.Models;

[Serializable, YamlSerializable]
public partial class PostFrontMatter : IValidatableObject
{
    [Required]
    public string LCID { get; set; }
    
    [Required]
    public string Layout { get; set; }
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public DateTime[] Timestamps { get; set; }
    
    [Required]
    public string[] Topic { get; set; }

    public PostFrontMatter(string lcid, string layout, string title, DateTime[] timestamps, string[] topic)
    {
        LCID = lcid;
        Layout = layout;
        Title = title;
        Timestamps = timestamps;
        Topic = topic;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(LCID)));
        list.AddRange(this.ValidateProperty(nameof(Layout)));
        list.AddRange(this.ValidateProperty(nameof(Title)));
        list.AddRange(this.ValidateProperty(nameof(Timestamps)));
        list.AddRange(this.ValidateProperty(nameof(Topic)));
        return list;
    }
}