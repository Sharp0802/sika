using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIKA.Components;

namespace SIKA.Models;

[Serializable]
public partial class HeaderInfo : IValidatableObject
{
    [JsonConstructor]
    public HeaderInfo(string title, string[] topics, DateTime[] timestamps)
    {
        Title      = title;
        Topics     = topics;
        Timestamps = timestamps;
    }

    [Required(AllowEmptyStrings = false)] public string Title { get; set; }

    [Required] public string[] Topics { get; set; }

    [Required] public DateTime[] Timestamps { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Title),      typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(Topics),     typeof(string[])));
        list.AddRange(this.ValidateProperty(nameof(Timestamps), typeof(DateTime[])));
        return list;
    }
}