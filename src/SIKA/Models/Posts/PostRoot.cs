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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace SIKA.Models.Posts;

public class PostRootImpl : PostTree
{
    protected PostRootImpl(FileSystemInfo? parent, FileSystemInfo current) : base(parent, current)
    {
    }

    public override XElement? GetHtml() => null;
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public class PostRoot(FileSystemInfo? parent, FileSystemInfo current) : PostRootImpl(parent, current)
{
    public new string GetHtml()
    {
        return Children
             ?.Order(PostTreeComparer.Default)
              .Select(child => child.GetHtml())
              .Aggregate(new StringBuilder(), (builder, html) => builder.AppendLine(html?.ToString()))
              .ToString()
               ?? string.Empty;
    }
}