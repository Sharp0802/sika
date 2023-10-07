using System.Collections;
using SIKA.Models.Abstraction;

namespace SIKA.Models.IO;

public class FileSystemTree(FileSystemInfo fsi, Predicate<FileInfo> query) : IReadOnlyTree<FileSystemTree>
{
    public FileSystemInfo Info { get; } = fsi;

    private Predicate<FileInfo> Query { get; } = query;

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