namespace BlogMan.Models.Abstraction;

public interface IReadOnlyTree<out TChild> : IEnumerable<TChild>
{
}