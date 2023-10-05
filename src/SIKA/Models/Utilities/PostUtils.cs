using SIKA.Models.Posts;

namespace SIKA.Models.Utilities;

public record PostConflict(
    IEnumerable<PostLeaf> Conflicts,
    string                Message
);

public static class PostUtils
{
    public static IEnumerable<PostConflict> Validate(this PostTree root)
    {
        return from leaf in root.Traverse().OfType<PostLeaf>()
               group leaf by leaf.GetIdentifier()
               into g
               where g.Count() > 1
               select new PostConflict(g, $"Duplicated post id: {g.Key}");
    }
}