using System.Reflection;
using System.Runtime.CompilerServices;
using BlogMan.Contexts;
using BlogMan.Models;
using Markdig;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Encoding = System.Text.Encoding;

namespace BlogMan.Components;

public sealed class Linker : IDisposable
{
    private readonly ThreadLocal<MarkdownPipeline> _pipeline = new();

    private MarkdownPipeline Pipeline => _pipeline.Value!;

    private readonly Dictionary<string, string> _escapedMap;
    private readonly Dictionary<string, string> _layoutMap;

    private readonly Project  _project;
    private readonly PostTree _tree;
    
    private static IRazorEngineService RazorService { get; }

    static Linker()
    {
        var config = new TemplateServiceConfiguration();
        RazorService = RazorEngineService.Create(config);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => RazorService.Dispose();
    }
    
    private Linker(Project project)
    {
        if (!Directory.Exists(project.Info.SiteDirectory))
            Directory.CreateDirectory(project.Info.SiteDirectory);
        _project = project;
        if ((_tree = BuildTree(project)!) is null)
            throw new InvalidDataException();
        var files = _tree.GetAllFile().ToArray();
        _escapedMap = files.ToDictionary(
            node => node.GetIdentifier(),
            node => node.GetEscapedIdentifier());
        if (!Directory.Exists(project.Info.LayoutDirectory))
            Directory.CreateDirectory(project.Info.LayoutDirectory);
#pragma warning disable CS8714
#pragma warning disable CS8619
        _layoutMap = Directory.EnumerateFiles(project.Info.LayoutDirectory).ToDictionary(
            Path.GetFileName,
            File.ReadAllText);
#pragma warning restore CS8619
#pragma warning restore CS8714

        var asmLoc = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
                     ?? throw new ReflectionTypeLoadException(
                         new[] { typeof(Assembly) },
                         null,
                         "Failed to load assembly");
        var wwwroot = Path.Combine(asmLoc, "wwwroot/");
        var resroot = Path.Combine(asmLoc, "Resources/");
        if (!_layoutMap.ContainsKey("default"))
            _layoutMap.Add("default", File.ReadAllText(resroot + "post.razor"));
        CopyDirectory(wwwroot, project.Info.SiteDirectory, true);


        _pipeline.Value = new MarkdownPipelineBuilder()
                         .UseAdvancedExtensions()
                         .UseYamlFrontMatter()
                         .UseUrlRewriter(url =>
                          {
                              if (url is null || !url.IsShortcut)
                                  return url?.UnescapedUrl.Text;

                              var t = url.UnescapedUrl.Text;
                              if (t.StartsWith("ref:"))
                                  t = t[4..];
                              if (_escapedMap.ContainsKey(t))
                                  t = _escapedMap[t];
                              else
                                  Logger.Log(LogLevel.WARN, $"Post '{t}' not found.");
                              return t;
                          })
                         .Build();
    }

    private static void CopyDirectory(string src, string dst, bool recurse)
    {
        var dir  = new DirectoryInfo(src);
        var dirs = dir.GetDirectories();
        Directory.CreateDirectory(dst);

        foreach (var file in dir.GetFiles())
        {
            var path = Path.Combine(dst, file.Name);
            if (File.Exists(path))
                File.Delete(path);
            file.CopyTo(path);
        }

        if (!recurse) return;

        foreach (var subDir in dirs)
        {
            var newDestinationDir = Path.Combine(dst, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }

    public static bool Link(Project project)
    {
        return new Linker(project).Link();
    }

    private bool Link()
    {
        var files = _tree.GetAllFile().ToArray();
        return files.AsParallel().Count(LinkFile) == files.Length;
    }

    private bool LinkFile(PostTreeNode node)
    {
        // if (node.File is not FileInfo info) return false;
        Unsafe.SkipInit(out string html);
        var ior = SEH.IO(node.File, info =>
        {
            var file = File.ReadAllText(info.FullName);
            html = Markdown.Parse(file, Pipeline).ToHtml(Pipeline);
        });
        if (!ior)
            return false;

        var htmlName = node.File.FullName;
        var yamlName = Path.ChangeExtension(htmlName, ".yaml");
        Console.WriteLine(yamlName);
        if (!File.Exists(yamlName))
        {
            Logger.Log(LogLevel.FAIL, "Failed to retrieve yaml front-matter");
            return false;
        }

        Unsafe.SkipInit(out PostFrontMatter metadata);
        ior = SEH.IO(yamlName, name =>
        {
            using var yamlStream = File.OpenRead(name);
            using var yamlReader = new StreamReader(yamlStream, Encoding.UTF8);
            metadata = Yaml.Deserialize<PostFrontMatter>(yamlReader);
        });
        if (!ior)
            return false;

        var errors = metadata.Validate().ToArray();
        if (errors.Length != 0)
        {
            Logger.Log(LogLevel.FAIL, "Invalid metadata detected.");
            errors.PrintErrors(node.File.FullName);
            return false;
        }

        node.Metadata = metadata;

        html = RazorService.RunCompile(
            _layoutMap[metadata.Layout],
            Guid.NewGuid().ToString(),
            typeof(TemplateModel),
            new TemplateModel(_project, metadata, _tree, html));

        ior = SEH.IO((object)null!, _ =>
        {
            var fdir = node.Parent is null
                ? node.File.Name switch
                {
                    "Error.md"   => Path.Combine(Path.GetDirectoryName(node.File.FullName)!, "404.md"),
                    "Welcome.md" => Path.Combine(Path.GetDirectoryName(node.File.FullName)!, "index.md"),
                    _            => node.File.FullName
                }
                : node.File.FullName;

            var fname = Path.GetRelativePath(_project.Info.BuildDirectory, fdir);
            fname = Path.ChangeExtension(fname, ".html");
            Console.WriteLine(fname);
            fname = Path.GetFullPath(fname, Path.GetFullPath(_project.Info.SiteDirectory));
            Console.WriteLine(fname);
            File.WriteAllText(fname, html, Encoding.UTF8);
        });
        if (!ior)
            return false;

        return true;
    }

    private static PostTree? BuildTree(Project proj)
    {
        var files = new DirectoryInfo(proj.Info.BuildDirectory).GetFiles("*.md");
        var welcome = files.SingleOrDefault(f => Path
                                                .GetFileNameWithoutExtension(f.Name)
                                                .Equals("welcome", StringComparison.OrdinalIgnoreCase));
        var error = files.SingleOrDefault(f => Path
                                              .GetFileNameWithoutExtension(f.Name)
                                              .Equals("error", StringComparison.OrdinalIgnoreCase));
        var roots = new DirectoryInfo(proj.Info.BuildDirectory)
                   .GetFileSystemInfos()
                   .Where(fs => (fs.Attributes & FileAttributes.Directory) != 0 || fs.FullName.EndsWith(".md"))
                   .Select(f => new PostTreeNode(f, null))
                   .ToArray();

        if (welcome is null)
        {
            Logger.Log(LogLevel.CRIT, "Cannot retrieve welcome page");
            return null;
        }

        if (error is null)
        {
            Logger.Log(LogLevel.CRIT, "Cannot retrieve error page");
            return null;
        }

        var welcomePage = new PostTreeNode(welcome, null);
        var errorPage   = new PostTreeNode(error,   null);

        return new PostTree(welcomePage, errorPage, roots);
    }

    public void Dispose()
    {
        _pipeline.Dispose();
    }
}