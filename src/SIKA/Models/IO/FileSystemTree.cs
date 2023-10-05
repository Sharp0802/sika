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

using System.Collections;
using SIKA.Models.Abstraction;

namespace SIKA.Models.IO;

public class FileSystemTree : IReadOnlyTree<FileSystemTree>
{
    public FileSystemTree(FileSystemInfo fsi, Predicate<FileInfo> query)
    {
        Info = fsi;
        Query = query;
    }

    public FileSystemInfo Info { get; }
    
    private Predicate<FileInfo> Query { get; }

    private IEnumerable<FileSystemTree> GetChildren()
    {
        return Info is not DirectoryInfo 
            ? Enumerable.Empty<FileSystemTree>() 
            : GetFileSystemInfos().Select(fsi => new FileSystemTree(fsi, Query));
    }

    private IEnumerable<FileSystemInfo> GetFileSystemInfos()
    {
        if (Info is not DirectoryInfo dir)
            return Enumerable.Empty<FileSystemInfo>();
        
        return dir
            .GetFileSystemInfos()
            .Where(fsi => fsi is DirectoryInfo || (fsi is FileInfo fi && Query(fi)));
    }
    
    public IEnumerator<FileSystemTree> GetEnumerator()
    {
        return GetChildren().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}