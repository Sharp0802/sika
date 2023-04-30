using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BlogMan.Components;

namespace BlogMan.Models;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public partial class PostTree : IValidatableObject
{
    public PostTree(PostTreeNode welcome, PostTreeNode error, PostTreeNode[] roots)
    {
        WelcomePage = welcome;
        ErrorPage   = error;
        Roots       = roots;
    }

    [Required] public PostTreeNode WelcomePage { get; }

    [Required] public PostTreeNode ErrorPage { get; }

    [Required] public PostTreeNode[] Roots { get; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(WelcomePage), typeof(PostTreeNode)));
        list.AddRange(this.ValidateProperty(nameof(ErrorPage),   typeof(PostTreeNode)));
        list.AddRange(this.ValidateProperty(nameof(Roots),       typeof(PostTreeNode[])));
        return list;
    }

    public IEnumerable<PostTreeNode> GetAllFile()
    {
        return Roots.SelectMany(root => root.GetAllFile());
    }

    public string GetHtml()
    {
        var builder = new StringBuilder();
        foreach (var root in Roots)
        {
            var errors = root.Validate().ToArray();
            if (errors.Length != 0)
            {
                errors.PrintErrors(root.File.Name, LogLevel.WARN);
                continue;
            }

            builder.Append(root.GetHtml());
        }

        return builder.ToString();
    }
}