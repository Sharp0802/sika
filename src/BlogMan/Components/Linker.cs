using System.Reflection;
using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BlogMan.Models;
using BlogMan.Structures;
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
        _layoutMap = Directory.EnumerateFiles(project.LayoutDirectory!).ToDictionary(
            dir => Path.GetFileName(dir).ToLowerInvariant(),
            dir => dir);
    }

    public static (int Success, int Total) Link(Project project)
    {
        if (!AssertProject(project))
        {
            Logger.Log(LogLevel.CRIT, "Invalid project property detected.");
            return (0, 0);
        }

        return new Linker(project).Link();
    }

    private (int Success, int Total) Link()
    {
        if (!AssertProject(_project))
        {
            Logger.Log(LogLevel.FAIL, "Invalid project declaration detected.");
            return (0, 0);
        }

        var files = _tree.GetAllFile().ToArray();
        return (files.AsParallel().Count(LinkFile), files.Length);
    }

    private static bool AssertProject(Project proj)
    {
        if (proj.ApiVersion is null)
        {
            Logger.Log(LogLevel.WARN, "Cannot retrieve the api version; Undefined behaviour can be occurred");
        }
        else if (proj.ApiVersion != Assembly.GetExecutingAssembly().GetName().Version)
        {
            Logger.Log(LogLevel.WARN, "Api version mismatched; Undefined behaviour can be occurred");
        }

        if (proj.Name is null)
        {
            Logger.Log(LogLevel.CRIT, "The property 'Name' has not found in project");
            return false;
        }

        if (proj.PostDirectory is null)
        {
            Logger.Log(LogLevel.WARN, "Post directory has not been set; Default value used");
            proj.PostDirectory = "post/";
        }

        if (proj.LayoutDirectory is null)
        {
            Logger.Log(LogLevel.WARN, "Layout directory has not been set; Default value used");
            proj.LayoutDirectory = "layout/";
        }

        if (proj.BuildDirectory is null)
        {
            Logger.Log(LogLevel.WARN, "Build directory has not been set; Default value used");
            proj.BuildDirectory = "obj/";
        }

        if (proj.SiteDirectory is null)
        {
            Logger.Log(LogLevel.WARN, "Site directory has not been set; Default value used");
            proj.SiteDirectory = "site/";
        }

        return SEH.IO(proj, static proj =>
        {
            if (!Directory.Exists(proj.PostDirectory))
                Directory.CreateDirectory(proj.PostDirectory!);
            if (!Directory.Exists(proj.LayoutDirectory))
                Directory.CreateDirectory(proj.LayoutDirectory!);
            if (!Directory.Exists(proj.BuildDirectory))
                Directory.CreateDirectory(proj.BuildDirectory!);
            if (!Directory.Exists(proj.SiteDirectory))
                Directory.CreateDirectory(proj.SiteDirectory!);
        });
    }

    private static bool AssertMetadata(PostTreeNode node, PostFrontMatter metadata)
    {
        var ret = true;

        if (metadata.Layout is null)
        {
            Logger.Log(LogLevel.WARN, "Layout cannot be null.", node.GetIdentifier());
            ret = false;
        }

        if (metadata.Date is null)
        {
            Logger.Log(LogLevel.WARN, "Date cannot be null.", node.GetIdentifier());
            ret = false;
        }

        if (metadata.Title is null)
        {
            Logger.Log(LogLevel.WARN, "Title cannot be null.", node.GetIdentifier());
            ret = false;
        }

        if (metadata.Topic is null)
        {
            Logger.Log(LogLevel.WARN, "Topic cannot be null.", node.GetIdentifier());
            ret = false;
        }

        return ret;
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

        if (!AssertMetadata(node, metadata))
        {
            Logger.Log(LogLevel.FAIL, "Invalid metadata detected.");
            return false;
        }

        html = Engine.Razor.RunCompile(
            _layoutMap[metadata.Layout!],
            node.GetIdentifier(),
            typeof(TemplateModel),
            new TemplateModel(metadata, html));

        ior = SEH.IO((object)null!, _ =>
        {
            var fname = Path.GetRelativePath(_project.BuildDirectory!, node.File.FullName);
            fname = Path.GetFullPath(fname, _project.SiteDirectory!);
            File.WriteAllText(fname, html, Encoding.UTF8);
        });
        if (!ior)
            return false;

        return true;
    }

    private static PostTree? BuildTree(Project proj)
    {
        var files = new DirectoryInfo(proj.BuildDirectory!).GetFiles();
        var welcome = files.SingleOrDefault(f => Path
            .GetFileNameWithoutExtension(f.Name)
            .Equals("welcome", StringComparison.OrdinalIgnoreCase));
        var error = files.SingleOrDefault(f => Path
            .GetFileNameWithoutExtension(f.Name)
            .Equals("error", StringComparison.OrdinalIgnoreCase));
        var roots = new DirectoryInfo(proj.PostDirectory!)
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