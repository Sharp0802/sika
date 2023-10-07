//     Copyright (C) 2023  Yeong-won Seo
// 
//     SIKA is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     SIKA is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics.Contracts;
using SIKA.Models.Abstraction;

namespace SIKA.Models.Utilities;

public enum TraversalOrder
{
    Preorder,
    Postorder
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
    private static TTo Burn<TFrom, TTo, TValue>(
        this TFrom                       current,
        TFrom                            parent,
        [Pure] Func<TFrom, TValue>       selector,
        [Pure] BurnDelegate<TValue, TTo> burner)
        where TFrom : IReadOnlyTree<TFrom>
        where TTo : ITree<TTo>
    {
        var root     = burner(selector(parent), selector(current));
        var children = current.Select(child => child.Burn(current, selector, burner));
        root.Set(children);
        return root;
    }

    [Pure]
    public static TTo Burn<TFrom, TTo, TValue>(
        this   TFrom                     from,
        [Pure] Func<TFrom, TValue>       selector,
        [Pure] BurnDelegate<TValue, TTo> burner)
        where TFrom : IReadOnlyTree<TFrom>
        where TTo : ITree<TTo>
    {
        var root     = burner(default, selector(from));
        var children = from.Select(child => child.Burn(from, selector, burner));
        root.Set(children);
        return root;
    }
}