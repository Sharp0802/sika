namespace SIKA.Models.Abstraction;

public interface ITree<TChild> : IReadOnlyTree<TChild>
{
    public void Set(IEnumerable<TChild> children);
}