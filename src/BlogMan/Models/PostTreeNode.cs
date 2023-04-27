using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Xml.Linq;
using BlogMan.Components;

namespace BlogMan.Models;

[Serializable]
public partial class PostTreeNode : IValidatableObject
{
    public PostTreeNode(FileSystemInfo file, PostTreeNode? parent)
    {
        File   = file;
        Parent = parent;
        Children = file is DirectoryInfo d
            ? d.GetFileSystemInfos().Select(f => new PostTreeNode(f, this)).ToArray()
            : Array.Empty<PostTreeNode>();
    }

    public FileSystemInfo File { get; }

    [Required] public PostFrontMatter? Metadata { get; set; }

    public PostTreeNode[] Children { get; }

    public PostTreeNode? Parent { get; }

    private string Name => (File.Attributes & FileAttributes.Directory) != 0
        ? File.Name
        : Path.ChangeExtension(File.Name, ".html");

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        var list = new List<ValidationResult>();
        list.AddRange(this.ValidateProperty(nameof(Metadata), typeof(PostFrontMatter)));
        return list;
    }

    private IEnumerable<PostTreeNode> GetUpperBranch()
    {
        var list = new Queue<PostTreeNode>();
        var node = this;
        do
        {
            list.Enqueue(node);
            node = node.Parent;
        } while (node is not null);

        return list.Reverse();
    }

    public string GetIdentifier()
    {
        return string.Join('/', GetUpperBranch().Select(node => node.Name));
    }

    public string GetEscapedIdentifier()
    {
        return string.Join("__", GetUpperBranch().Select(node => node.Name));
    }

    public IEnumerable<PostTreeNode> GetAllFile()
    {
        if (File is FileInfo)
            yield return this;
        foreach (var child in Children)
        foreach (var item in child.GetAllFile())
            yield return item;
    }


    public XElement? GetHtml()
    {
        XElement elem;

        if ((File.Attributes & FileAttributes.Directory) == 0)
        {
            if (Metadata is null)
                return null;
            if (Parent is null && Name.Equals("Error.html", StringComparison.OrdinalIgnoreCase))
                return null;

            var id = HttpUtility.HtmlAttributeEncode(GetEscapedIdentifier());
            elem = new XElement("li",
                new XElement("a",
                    new XAttribute("href", id),
                    new XText(Metadata?.Title ?? "<null>")
                )
            );
        }
        else
        {
            var ul = new XElement("ul",
                new XAttribute("class", "nested")
            );
            ul.Add(Children.Select(child => child.GetHtml()));
            elem = new XElement("li",
                new XElement("span",
                    new XAttribute("class", "caret"),
                    new XText(Path.GetFileNameWithoutExtension(Name))
                ),
                ul
            );
        }

        return elem;
    }
}