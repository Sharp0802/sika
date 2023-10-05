using System.ComponentModel.DataAnnotations;
using SIKA.Components;
using YamlDotNet.Serialization;

namespace SIKA.Models;

[Serializable]
[YamlSerializable]
public partial class PostFrontMatter : IValidatableObject
{
    public PostFrontMatter(string lcid, string layout, string title, DateTime[] timestamps, string[] topic)
    {
        LCID       = lcid;
        Layout     = layout;
        Title      = title;
        Timestamps = timestamps;
        Topic      = topic;
    }

    [Required] public string LCID { get; set; }

    [Required] public string Layout { get; set; }

    [Required] public string Title { get; set; }

    [Required]
    [Obsolete($"Do NOT use this code directly. Use {nameof(Timestamps)} instead.")]
    [YamlMember(Alias = nameof(Timestamps))]
    public string[] RawTimestamps { get; set; } = null!;

    [YamlIgnore]
    public DateTime[] Timestamps
    {
#pragma warning disable CS0618
        get => RawTimestamps.Select(t => DateTime.ParseExact(t, "yyyy-MM-dd", null)).ToArray();
        set => RawTimestamps = value.Select(t => t.ToString("yyyy-MM-dd")).ToArray();
#pragma warning restore CS0618
    }

    [Required] public string[] Topic { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(LCID),   typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Layout), typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Title),  typeof(string)));
#pragma warning disable CS0618
        list.AddRange(this.ValidateProperty(nameof(RawTimestamps), typeof(string[])));
#pragma warning restore CS0618
        list.AddRange(this.ValidateProperty(nameof(Topic), typeof(string[])));
        return list;
    }
}