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