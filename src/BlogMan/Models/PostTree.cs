using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlogMan.Models;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public partial class PostTree
{
    public PostTree(PostTreeNode[] nodes)
    {
        Nodes = nodes;
    }

    public PostTreeNode[] Nodes { get; }

    public IEnumerable<PostTreeNode> GetAllFile()
    {
        return Nodes.SelectMany(root => root.GetAllFile());
    }

    public string GetHtml()
    {
        var builder = new StringBuilder();
        foreach (var root in Nodes)
            builder.Append(root.GetHtml());

        return builder.ToString();
    }
}