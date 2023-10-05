using System.Collections;
using BlogMan.Models.Abstraction;

namespace BlogMan.Models.IO;

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