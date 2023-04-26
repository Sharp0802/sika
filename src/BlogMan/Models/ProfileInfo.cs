using System.ComponentModel.DataAnnotations;
using BlogMan.Components;
using YamlDotNet.Serialization;

namespace BlogMan.Models;

[Serializable]
[YamlSerializable]
public partial class ProfileInfo : IValidatableObject
{
    public ProfileInfo(string name, string image)
    {
        UserName     = name;
        ProfileImage = image;
    }

    [Required(AllowEmptyStrings = false)] public string UserName { get; set; }

    [Required(AllowEmptyStrings = false)] public string ProfileImage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(UserName)));
        list.AddRange(this.ValidateProperty(nameof(ProfileImage)));
        return list;
    }
}