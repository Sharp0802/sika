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