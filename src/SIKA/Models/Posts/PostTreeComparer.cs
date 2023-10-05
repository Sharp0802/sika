//    Copyright (C) 2023  Yeong-won Seo
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

namespace SIKA.Models.Posts;

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