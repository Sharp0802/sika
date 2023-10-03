using System.Xml.Linq;
using BlogMan.Components;
using BlogMan.Models.Utilities;

namespace BlogMan.Models.Posts;

public enum PostKind
{
    None,
    Welcome,
    Error
}

public class PostLeaf : PostTree
{
    public DirectoryInfo    Directory { get; }
    public FileInfo         Info      { get; }
    public PostFrontMatter? Metadata  { get; }
    public PostKind         Kind      { get; }

    public string Title => Metadata?.Title ?? Path.GetFileNameWithoutExtension(Info.Name);

    public PostLeaf(FileSystemInfo parent, FileSystemInfo current) : base(parent, current)
    {
        Directory = (DirectoryInfo)parent;
        Info      = (FileInfo)current;
        Metadata  = GetFrontMatter();

        Kind = current.Name.ToLowerInvariant() switch
        {
            "welcome.md" => PostKind.Welcome,
            "error.md"   => PostKind.Error,
            _            => PostKind.None
        };
    }

    private PostFrontMatter? GetFrontMatter()
    {
        var name = Path.GetFileNameWithoutExtension(Info.Name);

        var q =
            from fsi in Directory.EnumerateFiles()
            let ext = fsi.Extension
            let cmp = Path.GetFileNameWithoutExtension(fsi.Name)
            where ext.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
                  ext.Equals(".yml",  StringComparison.OrdinalIgnoreCase)
            where name.Equals(cmp, StringComparison.Ordinal)
            select fsi;

        var file = q.FirstOrDefault();
        if (file is null)
            return null;

        using var stream = file.OpenText();
        return Yaml.Deserialize<PostFrontMatter>(stream);
    }

    public string GetIdentifier()
    {
        return UriUtils.Normalize(Current.Name);
    }

    public string GetHRef()
    {
        return $"/{GetIdentifier()}";
    }

    public override XElement? GetHtml()
    {
        if (Parent is null && Kind == PostKind.Error)
            return null;

        return new XElement("li",
            new XElement("a",
                new XAttribute("href", GetHRef()),
                new XText(Title)
            )
        );
    }
}