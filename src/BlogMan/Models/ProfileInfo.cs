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

    [Required] public string UserName { get; set; }

    [Required] public string ProfileImage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(UserName), typeof(string)));
        list.AddRange(this.ValidateProperty(nameof(ProfileImage), typeof(string)));
        return list;
    }
}