namespace SIKA.Models.Abstraction;

public interface IReadOnlyTree<out TChild> : IEnumerable<TChild>;
