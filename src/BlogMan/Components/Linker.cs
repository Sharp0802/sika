using System.Reflection;
using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BlogMan.Models;
using RazorEngine;
using RazorEngine.Templating;
using YamlDotNet.Serialization;
using Encoding = System.Text.Encoding;

namespace BlogMan.Components;

public class Linker
{
    [ThreadStatic] private static IDeserializer?             _deserializer;
    private readonly              Dictionary<string, string> _escapedMap;
    private readonly              Dictionary<string, string> _layoutMap;

    private readonly Project  _project;
    private readonly PostTree _tree;


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
    }

    private static IDeserializer Deserializer => _deserializer ??= new Deserializer();

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
            using var file = File.OpenRead(info.FullName);
            using var docs = new HtmlParser().ParseDocument(file);
            foreach (var link in docs.QuerySelectorAll("a").OfType<IHtmlLinkElement>())
            {
                if (link.Href is null || !link.Href.StartsWith("ref:")) continue;
                var href = link.Href[4..];
                if (_escapedMap.ContainsKey(href!))
                    link.Href = _escapedMap[href];
                else
                    Logger.Log(LogLevel.WARN, $"Post '{href}' not found.");
            }

            html = docs.ToHtml(new PrettyMarkupFormatter());
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
            metadata = Deserializer.Deserialize<PostFrontMatter>(yamlReader);
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

        html = Engine.Razor.RunCompile(
            _layoutMap[metadata.Layout],
            Guid.NewGuid().ToString(),
            typeof(TemplateModel),
            new TemplateModel(_project, metadata, _tree, html));

        ior = SEH.IO((object)null!, _ =>
        {
            var fname = Path.GetRelativePath(
                _project.Info.BuildDirectory,
                node.Parent is null && node.File.Name.Equals("Error.html", StringComparison.OrdinalIgnoreCase)
                    ? Path.Combine(Path.GetDirectoryName(node.File.FullName)!, "404.html")
                    : node.File.FullName);
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
        var files = new DirectoryInfo(proj.Info.BuildDirectory).GetFiles("*.html");
        var welcome = files.SingleOrDefault(f => Path
                                                .GetFileNameWithoutExtension(f.Name)
                                                .Equals("welcome", StringComparison.OrdinalIgnoreCase));
        var error = files.SingleOrDefault(f => Path
                                              .GetFileNameWithoutExtension(f.Name)
                                              .Equals("error", StringComparison.OrdinalIgnoreCase));
        var roots = new DirectoryInfo(proj.Info.BuildDirectory)
                   .GetFileSystemInfos()
                   .Where(fs => (fs.Attributes & FileAttributes.Directory) != 0 || fs.FullName.EndsWith(".html"))
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
}