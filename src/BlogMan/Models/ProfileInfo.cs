using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class ProfileInfo : IValidatableObject
{
    [JsonConstructor]
    public ProfileInfo(string userName, string profileImage)
    {
        UserName     = userName;
        ProfileImage = profileImage;
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