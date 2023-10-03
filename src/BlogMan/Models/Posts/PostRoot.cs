using System.Text;
using System.Xml.Linq;

namespace BlogMan.Models.Posts;

public class PostRootImpl : PostTree
{
    public PostRootImpl(FileSystemInfo? parent, FileSystemInfo current) : base(parent, current)
    {
    }

    public override XElement? GetHtml() => null;
}

public class PostRoot : PostRootImpl
{
    public PostRoot(FileSystemInfo? parent, FileSystemInfo current) : base(parent, current)
    {
    }

    public new string GetHtml()
    {
        return Children
                   ?.Select(child => child.GetHtml())
                   .Aggregate(new StringBuilder(), (builder, html) => builder.AppendLine(html?.ToString()))
                   .ToString()
               ?? string.Empty;
    }
}