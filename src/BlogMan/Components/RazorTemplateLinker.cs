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

    protected override bool Link(LinkerEventArgs args)
    {
        var errors = args.PostNode.Metadata.Validate().ToArray();
        if (errors.Length != 0)
        {
            Logger.Log(LogLevel.FAIL, "Invalid metadata detected.");
            errors.PrintErrors(args.PostNode.Identifier);
            return false;
        }

        var html = RazorService.RunCompile(
            GetLayout(args.PostNode.Metadata.Layout),
            Guid.NewGuid().ToString(),
            typeof(TemplateModel),
            new TemplateModel(args.Project, args.PostNode.Metadata, args.PostTree, args.Content));

        using var fs = args.Destination.Open(FileMode.Create, FileAccess.Write);
        using var sw = new StreamWriter(fs, Encoding.UTF8);

        sw.Write(html);

        return true;
    }

    public void Dispose()
    {
        _pipeline.Dispose();
    }
}