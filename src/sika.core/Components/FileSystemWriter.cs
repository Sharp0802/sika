// Copyright (C)  2024  Yeong-won Seo
// 
// SIKA is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SIKA is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using sika.core.Components.Abstract;
using sika.core.Model;

namespace sika.core.Components;

public class FileSystemWriter(Project project) : IWriter
{
    public async Task WriteAsync(PageTree tree)
    {
        var fullname = Path.Combine(project.Info.SiteDirectory, tree.GetFullPath());

        if (tree.Content is PageLeafData leaf)
        {
            fullname = Path.ChangeExtension(fullname, ".html");
            var             file = new FileInfo(fullname);
            await using var fs   = file.OpenWrite();
            fs.Write(Encoding.UTF8.GetBytes(leaf.Content));
        }
        else
        {
            var dir = new DirectoryInfo(fullname);
            if (!dir.Exists)
                dir.Create();
        }
    }
}