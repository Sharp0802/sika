using System.ComponentModel.DataAnnotations;
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
    [ThreadStatic] private static IDeserializer? _deserializer;

    private static IDeserializer Deserializer => _deserializer ??= new Deserializer();

    private readonly Project _project;
    private readonly PostTree _tree;
    private readonly Dictionary<string, string> _escapedMap;
    private readonly Dictionary<string, string> _layoutMap;

    private Linker(Project project)
    {
        _project = project;
        if ((_tree = BuildTree(project)!) is null)
            throw new InvalidDataException();
        var files = _tree.GetAllFile().ToArray();
        _escapedMap = files.ToDictionary(
            node => node.GetIdentifier(),
            node => node.GetEscapedIdentifier());
        _layoutMap = Directory.EnumerateFiles(project.Info.LayoutDirectory).ToDictionary(
            dir => Path.GetFileName(dir).ToLowerInvariant(),
            dir => dir);
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

        var htmlname = node.File.FullName;
        var yamlname = Path.ChangeExtension(htmlname, ".yaml");
        if (!File.Exists(yamlname))
        {
            Logger.Log(LogLevel.FAIL, "Failed to retrieve yaml front-matter");
            return false;
        }

        Unsafe.SkipInit(out PostFrontMatter metadata);
        ior = SEH.IO(yamlname, name =>
        {
            using var yamlstream = File.OpenRead(name);
            using var yamlreader = new StreamReader(yamlstream, Encoding.UTF8);
            metadata = Deserializer.Deserialize<PostFrontMatter>(yamlreader);
        });
        if (!ior)
            return false;

        var errors = metadata.Validate().ToArray();
        if (errors.Any())
        {
            Logger.Log(LogLevel.FAIL, "Invalid metadata detected.");
            errors.PrintErrors(node.File.FullName);
            return false;    
        }

        node.Metadata = metadata;

        html = Engine.Razor.RunCompile(
            _layoutMap[metadata.Layout],
            node.GetIdentifier(),
            typeof(TemplateModel),
            new TemplateModel(_project, metadata, _tree, html));

        ior = SEH.IO((object)null!, _ =>
        {
            var fname = Path.GetRelativePath(_project.Info.BuildDirectory, node.File.FullName);
            fname = Path.GetFullPath(fname, _project.Info.SiteDirectory);
            File.WriteAllText(fname, html, Encoding.UTF8);
        });
        if (!ior)
            return false;

        return true;
    }

    private static PostTree? BuildTree(Project proj)
    {
        var files = new DirectoryInfo(proj.Info.BuildDirectory).GetFiles();
        var welcome = files.SingleOrDefault(f => Path
            .GetFileNameWithoutExtension(f.Name)
            .Equals("welcome", StringComparison.OrdinalIgnoreCase));
        var error = files.SingleOrDefault(f => Path
            .GetFileNameWithoutExtension(f.Name)
            .Equals("error", StringComparison.OrdinalIgnoreCase));
        var roots = new DirectoryInfo(proj.Info.PostDirectory)
            .GetFileSystemInfos()
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
        var errorPage = new PostTreeNode(error, null);

        return new PostTree(welcomePage, errorPage, roots);
    }
}