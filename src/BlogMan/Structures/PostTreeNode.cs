using System.Text;

namespace BlogMan.Structures;

public class PostTreeNode
{
    public FileSystemInfo File { get; }
    public PostTreeNode[] Children { get; private set; }
    public PostTreeNode? Parent { get; }

    public PostTreeNode(FileSystemInfo file, PostTreeNode? parent)
    {
        File = file;
        Parent = parent;
        Children = file is DirectoryInfo d
            ? d.GetFileSystemInfos().Select(f => new PostTreeNode(f, this)).ToArray()
            : Array.Empty<PostTreeNode>();
    }

    public string GetIdentifier()
    {
        var builder = new StringBuilder();
        var node = this;
        do
        {
            builder.Append(node.File.Name).Append('/');
            node = node.Parent;
        } while (node != null);

        builder.Append(File.Name);

        return builder.ToString();
    }

    public string GetEscapedIdentifier()
    {
        var builder = new StringBuilder();
        var node = this;
        do
        {
            builder.Append(node.File.Name).Append("__");
            node = node.Parent;
        } while (node != null);

        builder.Append(File.Name);

        return builder.ToString();
    }

    public IEnumerable<PostTreeNode> GetAllFile()
    {
        if (File is FileInfo)
            yield return this;
        foreach (var child in Children)
        foreach (var item in child.GetAllFile())
            yield return item;
    }
}