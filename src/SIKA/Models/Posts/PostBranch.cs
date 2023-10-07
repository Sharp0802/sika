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

using System.Xml.Linq;

namespace SIKA.Models.Posts;

public class PostBranch(FileSystemInfo parent, FileSystemInfo current) : PostTree(parent, current)
{
    public new DirectoryInfo Current { get; } = (DirectoryInfo)current;

    public override XElement? GetHtml()
    {
        if (Children is null)
            return null;

        return new XElement("li",
            new XElement("span",
                new XAttribute("class", "caret"),
                new XText(Current.Name)
            ),
            new XElement("ul",
                Children
                   .Order(PostTreeComparer.Default)
                   .Select(child => child.GetHtml() as XObject)
                   .Prepend(new XAttribute("class", "nested"))
            )
        );
    }
}