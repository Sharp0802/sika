using System.Collections.Concurrent;
using System.Text;
using BlogMan.Models;
using Markdig;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace BlogMan.Components;

public sealed class RazorTemplateLinker : LinkerBase, IDisposable
{
    private readonly ConcurrentDictionary<string, string> _layoutMap = new();
    private readonly ThreadLocal<MarkdownPipeline>        _pipeline  = new();

    public RazorTemplateLinker(Project project) : base(project)
    {
        var config = new TemplateServiceConfiguration();
        RazorService = RazorEngineService.Create(config);
    }

    private IRazorEngineService RazorService { get; }

    public override void Dispose()
    {
        _pipeline.Dispose();
    }

    private string GetLayout(string key)
    {
        return _layoutMap.GetOrAdd(key, p =>
        {
            p = Path.ChangeExtension(p, ".razor");
            return File.ReadAllText(p.ToLowerInvariant().Equals("default.razor", StringComparison.Ordinal)
                ? Path.Combine(AppContext.BaseDirectory,     "Resources/post.razor")
                : Path.Combine(Project.Info.LayoutDirectory, p));
        });
    }


    protected override bool Initialize()
    {
        return SEH.IO(Path.Combine(AppContext.BaseDirectory, "wwwroot"), dir =>
        {
            var dst = new DirectoryInfo(Project.Info.SiteDirectory);
            if (dst.Exists)
                dst.Create();
            new DirectoryInfo(dir).CopyTo(dst);
        });
    }

    protected override bool Link(LinkerEventArgs args)
    {
        var metadata = args.PostNode.FrontMatter;
        if (metadata is null)
        {
            Logger.Log(LogLevel.FAIL, "Metadata not found.");
            return false;
        }
        
        var errors = metadata.Validate().ToArray();
        if (errors.Length != 0)
        {
            Logger.Log(LogLevel.FAIL, "Invalid metadata detected.");
            errors.PrintErrors(args.PostNode.GetIdentifier());
            return false;
        }

        var html = RazorService.RunCompile(
            GetLayout(metadata.Layout),
            Guid.NewGuid().ToString(),
            typeof(TemplateModel),
            new TemplateModel(args.Project, metadata, args.PostTree, args.Content));

        using var fs = args.Destination.Open(FileMode.Create, FileAccess.Write);
        using var sw = new StreamWriter(fs, Encoding.UTF8);

        sw.Write(html);

        return true;
    }
}