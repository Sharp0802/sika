namespace BlogMan.Models.Posts;

public class PostTreeComparer : IComparer<PostTree>
{
    public static PostTreeComparer Default => new();

    private static bool IsGreaterThan(PostTree? x, PostTree? y)
    {
        if (x is null)
            return false;
        if (y is null)
            return true;

        switch (x)
        {
            case PostBranch bx:
            {
                if (y is not PostBranch by) 
                    return true;
                return string.CompareOrdinal(bx.Current.Name, by.Current.Name) > 0;
            }
            case PostLeaf lx:
            {
                if (y is not PostLeaf ly)
                    return false;
                return string.CompareOrdinal(lx.GetIdentifier(), ly.GetIdentifier()) > 0;
            }
            default:
                return false;
        }
    }
    
    public int Compare(PostTree? x, PostTree? y)
    {
        if (IsGreaterThan(x, y))
            return 1;
        if (IsGreaterThan(y, x))
            return -1;
        return 0;
    }
}