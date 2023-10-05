using System.Xml.Linq;

namespace SIKA.Models.Posts;

public class PostBranch(FileSystemInfo parent, FileSystemInfo current) : PostTree(parent, current)
{
    public new DirectoryInfo Current { get; } = (DirectoryInfo)current;

    public override XElement? GetHtml()
    {
        if (Children is null)
            return null;

        return new XElement("li",
            new XElement("span",
                new XAttribute("class", "caret"),
                new XText(Current.Name)
            ),
            new XElement("ul",
                Children
                   .Order(PostTreeComparer.Default)
                   .Select(child => child.GetHtml() as XObject)
                   .Prepend(new XAttribute("class", "nested"))
            )
        );
    }
}