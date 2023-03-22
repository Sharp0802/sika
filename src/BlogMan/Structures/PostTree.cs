namespace BlogMan.Structures;

public class PostTree
{
    public PostTreeNode WelcomePage { get; }
    public PostTreeNode ErrorPage { get; }
    public PostTreeNode[] Roots { get; }

    public PostTree(PostTreeNode welcome, PostTreeNode error, PostTreeNode[] roots)
    {
        WelcomePage = welcome;
        ErrorPage = error;
        Roots = roots;
    }

    public IEnumerable<PostTreeNode> GetAllFile()
    {
        return Roots.SelectMany(root => root.GetAllFile());
    }
}