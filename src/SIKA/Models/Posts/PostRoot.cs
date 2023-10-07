using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace SIKA.Models.Posts;

public class PostRootImpl : PostTree
{
    protected PostRootImpl(FileSystemInfo? parent, FileSystemInfo current) : base(parent, current)
    {
    }

    public override XElement? GetHtml() => null;
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public class PostRoot(FileSystemInfo? parent, FileSystemInfo current) : PostRootImpl(parent, current)
{
    public new string GetHtml()
    {
        return Children
             ?.Order(PostTreeComparer.Default)
              .Select(child => child.GetHtml())
              .Aggregate(new StringBuilder(), (builder, html) => builder.AppendLine(html?.ToString()))
              .ToString()
               ?? string.Empty;
    }
}