using System.Reflection;
using System.Text;
using BlogMan.Models;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace BlogMan.Components;

public class Compiler
{
    [field: ThreadStatic] private static MarkdownPipeline? _pipeline;
    
    private static MarkdownPipeline Pipeline => _pipeline ??= new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build();

    public static (int Total, int Success) Compile(Project proj)
    {
        if (proj.ApiVersion is null)
        {
            Logger.Log(LogLevel.WARN, "Cannot retrieve the api version");
        }
        else if (proj.ApiVersion != Assembly.GetExecutingAssembly().GetName().Version)
        {
            Logger.Log(LogLevel.WARN, "Api version mismatched; Undefined behaviour can be occurred");
        }
        
        if (proj.Name is null)
        {
            Logger.Log(LogLevel.CRIT, "The property 'Name' has not found in project");
            return (0, 0);
        }

        if (proj.PostDirectory is null)
        {
            Logger.Log(LogLevel.WARN, "Post directory has not been set; Default value used");
            proj.PostDirectory = "post/";
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

        var total = 0;
        var success = 0;
        Logger.Log(LogLevel.INFO, "Compiling start", proj.Name);
        Parallel.ForEach(EnumerateFiles(new DirectoryInfo(proj.PostDirectory)), file =>
        {
            Logger.Log(LogLevel.INFO, "Compiling start", file.FullName);
            Interlocked.Increment(ref total);
            if (Compile(file.FullName, proj))
            {
                Interlocked.Increment(ref success);
                Logger.Log(LogLevel.CMPL, "Success to compile", file.FullName);
            }
            else
            {
                Logger.Log(LogLevel.FAIL, "Failed to compile", file.FullName);
            }
        });
        Logger.Log(LogLevel.CMPL, $"Compiling complete ({success}/{total})", proj.Name);

        return (total, success);
    }

    private static IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo dir)
    {
        foreach (var f in dir.EnumerateFiles())
            yield return f;
        foreach (var d in dir.EnumerateDirectories())
        foreach (var f in EnumerateFiles(d))
            yield return f;
    }

    private static bool Compile(string file, Project proj)
    {
        var dst = Path.GetRelativePath(proj.PostDirectory!, file);
        dst = Path.GetFullPath(proj.BuildDirectory!, dst);
        var dstyaml = Path.ChangeExtension(dst, ".yaml");
        var dsthtml = Path.ChangeExtension(dst, ".html");

        return SEH.IO((Yaml: dstyaml, Html: dsthtml), args =>
        {
            var dstdir = new DirectoryInfo(Path.GetDirectoryName(dst)!);
            if (!dstdir.Exists) dstdir.Create();

            var md = Markdown.Parse(File.ReadAllText(file), Pipeline);
            Parallel.Invoke(
                () =>
                {
                    if (md.Descendants<YamlFrontMatterBlock>().FirstOrDefault() is { } yaml)
                    {
                        File.WriteAllText(dstyaml, dstyaml, Encoding.UTF8);
                    }
                    else
                    {
                        Logger.Log(LogLevel.WARN, "Yaml front-matter not found.", file);
                    }
                },
                () => File.WriteAllText(dsthtml, md.ToHtml(Pipeline), Encoding.UTF8)
            );
        });
    }
}