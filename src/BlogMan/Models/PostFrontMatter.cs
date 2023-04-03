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

    [Required, 
     Obsolete($"Do NOT use this code directly. Use {nameof(Timestamps)} instead."), 
     YamlMember(Alias = nameof(Timestamps))] 
    public string[] RawTimestamps { get; set; } = null!;
    
    [YamlIgnore]
    public DateTime[] Timestamps
    {
#pragma warning disable CS0618
        get => RawTimestamps.Select(t => DateTime.ParseExact(t, "yyyy-MM-dd", null)).ToArray();
        set => RawTimestamps = value.Select(t => t.ToString("yyyy-MM-dd")).ToArray();
#pragma warning restore CS0618
    }

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
#pragma warning disable CS0618
        list.AddRange(this.ValidateProperty(nameof(RawTimestamps)));
#pragma warning restore CS0618
        list.AddRange(this.ValidateProperty(nameof(Topic)));
        return list;
    }
}