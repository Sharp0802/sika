using System.Security.Cryptography;
using System.Xml.Linq;
using BlogMan.Components;
using Encoding = System.Text.Encoding;

namespace BlogMan.Models;

public partial class PostTreeNode
{
    private PostFrontMatter? _metadata;
    private string?          _identifier;
    private string?          _htmlIdentifier;

    public PostTreeNode(FileSystemInfo file, PostTreeNode? parent)
    {
        FileRecord = file;
        Parent     = parent;
        Children = file is DirectoryInfo dir
            ? dir.GetFileSystemInfos()
                 .Where(info => (info.Attributes & FileAttributes.Directory) != 0 ||
                                info.Extension.ToLowerInvariant().Equals(".html", StringComparison.Ordinal))
                 .Select(info => new PostTreeNode(info, this))
                 .ToArray()
            : Array.Empty<PostTreeNode>();
    }


    public FileSystemInfo FileRecord { get; }
    public PostTreeNode?  Parent     { get; }

    public string Identifier => _identifier ??= GetIdentifier();

    public string HtmlIdentifier
    {
        get
        {
            if (_htmlIdentifier is not null)
                return _htmlIdentifier;

            if (Parent is null)
            {
                var name = Path.GetFileNameWithoutExtension(FileRecord.Name).ToLowerInvariant();
                if (name.Equals("welcome", StringComparison.Ordinal))
                    return _htmlIdentifier = "index.html";
                if (name.Equals("error", StringComparison.Ordinal))
                    return _htmlIdentifier = "404.html";
            }
            
            var buf = Encoding.UTF8.GetBytes(GetIdentifier());
            buf = SHA1.HashData(buf);
            return _htmlIdentifier = $"{Convert.ToHexString(buf).ToUpperInvariant()}.html";
        }
    }

    public PostFrontMatter Metadata => _metadata ??= QueryMetadata();


    private PostTreeNode[] Children { get; }

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

    private string GetIdentifier()
    {
        return string.Join('/', GetUpperBranch().Select(node => node.FileRecord.Name));
    }

    public IEnumerable<PostTreeNode> GetAllFile()
    {
        if (FileRecord is FileInfo)
            yield return this;
        foreach (var child in Children)
        foreach (var item in child.GetAllFile())
            yield return item;
    }


    private PostFrontMatter QueryMetadata()
    {
        if (FileRecord is not FileInfo file)
            throw new InvalidOperationException();
        var metadata = file.Directory
                            !.EnumerateFiles()
                           .SingleOrDefault(info => Path.GetFileNameWithoutExtension(info.Name).Equals(
                                                        Path.GetFileNameWithoutExtension(file.Name),
                                                        StringComparison.Ordinal) &&
                                                    info.Extension.ToLowerInvariant()
                                                        .Equals(".yaml", StringComparison.Ordinal));
        if (metadata is null)
            throw new FileNotFoundException();

        using var fs = metadata.OpenRead();
        using var sr = new StreamReader(fs, Encoding.UTF8);

        return Yaml.Deserialize<PostFrontMatter>(sr);
    }


    public XElement? GetHtml()
    {
        XElement elem;

        if ((FileRecord.Attributes & FileAttributes.Directory) == 0)
        {
            if (Parent is null && FileRecord.Name.ToLowerInvariant().Equals("error.html", StringComparison.Ordinal))
                return null;

            elem = new XElement("li",
                new XElement("a",
                    new XAttribute("href", HtmlIdentifier),
                    new XText(Metadata.Title)
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
                    new XText(Path.GetFileNameWithoutExtension(FileRecord.Name))
                ),
                ul
            );
        }

        return elem;
    }
}