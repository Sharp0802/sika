using System.Collections;
using System.Xml.Linq;
using BlogMan.Models.Abstraction;
using BlogMan.Models.IO;
using BlogMan.Models.Utilities;

namespace BlogMan.Models.Posts;

public abstract class PostTree : ITree<PostTree>
{
    protected PostTree[]? Children { get; private set; }
    protected FileSystemInfo? Parent { get; }
    protected FileSystemInfo Current { get; }

    protected PostTree(FileSystemInfo? parent, FileSystemInfo current)
    {
        Parent = parent;
        Current = current;
    }

    public static PostRoot Burn(FileSystemTree fst)
    {
        var fsiRoot = fst.Info;
        return (PostRoot)fst.Burn(
            static fst => fst.Info,
            (p, cur) =>
            {
                if (p is null)
                {
                    if (cur == fsiRoot) 
                        return new PostRoot(null, cur) as PostTree;
                    throw new NullReferenceException("Parent node cannot be null");
                }

                if ((cur.Attributes & FileAttributes.Directory) != 0)
                    return new PostBranch(p, cur);
                return new PostLeaf(p, cur);
            });
    }

    public abstract XElement? GetHtml();
    
    public void Set(IEnumerable<PostTree> children)
    {
        Children = children.ToArray();
    }

    public IEnumerator<PostTree> GetEnumerator()
    {
        return Children is null
            ? Enumerable.Empty<PostTree>().GetEnumerator() 
            : ((IEnumerable<PostTree>)Children).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}