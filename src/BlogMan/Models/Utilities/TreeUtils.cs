using System.Diagnostics.Contracts;
using BlogMan.Models.Abstraction;

namespace BlogMan.Models.Utilities;

public enum TraversalOrder
{
    Preorder,
    Postorder,
}

public static class TreeUtils
{
    public static IEnumerable<T> Traverse<T>(this T root, TraversalOrder order = TraversalOrder.Preorder)
        where T : IReadOnlyTree<T>
    {
        switch (order)
        {
            case TraversalOrder.Preorder:
            {
                yield return root;
                foreach (var child in root)
                foreach (var desc in child.Traverse(order))
                    yield return desc;
                break;
            }
            case TraversalOrder.Postorder:
            {
                foreach (var child in root)
                foreach (var desc in child.Traverse(order))
                    yield return desc;
                yield return root;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(order), order, null);
        }
    }

    public delegate TTo BurnDelegate<in TFrom, out TTo>(TFrom? parent, TFrom current);
    
    [Pure]
    public static TTo Burn<TFrom, TTo, TValue>(
        this TFrom from,
        [Pure] Func<TFrom, TValue> selector,
        [Pure] BurnDelegate<TValue, TTo> burner)
        where TFrom : IReadOnlyTree<TFrom>
        where TTo : ITree<TTo>
    {
        var root = burner(default, selector(from));
        var children = from.Select(child => child.Burn(selector, burner));
        root.Set(children);
        return root;
    }
}