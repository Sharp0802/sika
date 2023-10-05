using System.Xml.Linq;

namespace BlogMan.Models.Posts;

public class PostBranch : PostTree
{
    private new DirectoryInfo Current { get; }

    public PostBranch(FileSystemInfo parent, FileSystemInfo current) : base(parent, current)
    {
        Current = (DirectoryInfo)current;
    }

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
                    .Select(child => child.GetHtml() as XObject)
                    .Prepend(new XAttribute("class", "nested"))
            )
        );
    }
}