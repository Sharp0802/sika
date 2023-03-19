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

    public static void Compile(Project proj)
    {
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
        var dst = Path.GetRelativePath(proj.PostDirectory, file);
        dst = Path.GetFullPath(proj.BuildDirectory, dst);
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