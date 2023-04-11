using System.Text;
using BlogMan.Models;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace BlogMan.Components;

public static class Compiler
{
    [field: ThreadStatic] private static MarkdownPipeline? _pipeline;
    
    private static MarkdownPipeline Pipeline => _pipeline ??= new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build();

    public static bool Compile(Project proj)
    {
        var total = 0;
        var success = 0;
        Logger.Log(LogLevel.INFO, "Compiling start", proj.Info.Name);
        Parallel.ForEach(EnumerateFiles(new DirectoryInfo(proj.Info.PostDirectory)), file =>
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

        return total == success;
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
        var dst = Path.GetRelativePath(proj.Info.PostDirectory, file);
        dst = Path.GetFullPath(dst, Path.GetFullPath(proj.Info.BuildDirectory));
        var dstYaml = Path.ChangeExtension(dst, ".yaml");
        var dstHtml = Path.ChangeExtension(dst, ".html");

        return SEH.IO((Yaml: dstYaml, Html: dstHtml), args =>
        {
            var dstInfo = new DirectoryInfo(Path.GetDirectoryName(dst)!);
            if (!dstInfo.Exists) dstInfo.Create();

            var txt = File.ReadAllText(file);
            var md = Markdown.Parse(txt, Pipeline);
            Parallel.Invoke(
                () =>
                {
                    if (md.Descendants<YamlFrontMatterBlock>().FirstOrDefault() is { } yaml)
                    {
                        var yamlText = txt.Substring(yaml.Span.Start, yaml.Span.Length);
                        yamlText = yamlText
                            .Split('\n')
                            .Skip(1)
                            .SkipLast(1)
                            .Aggregate(new StringBuilder(), (builder, str) => builder.AppendLine(str))
                            .ToString();
                        File.WriteAllText(args.Yaml, yamlText, Encoding.UTF8);
                    }
                    else
                    {
                        Logger.Log(LogLevel.WARN, "Yaml front-matter not found.", file);
                    }
                },
                () => File.WriteAllText(args.Html, md.ToHtml(Pipeline), Encoding.UTF8)
            );
        });
    }
}