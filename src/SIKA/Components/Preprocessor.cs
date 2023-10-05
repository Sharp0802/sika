//    Copyright (C) 2023  Yeong-won Seo
// 
//     SIKA is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     SIKA is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using SIKA.Models;

namespace SIKA.Components;

public static class Preprocessor
{
    [field: ThreadStatic] private static MarkdownPipeline? _pipeline;

    private static MarkdownPipeline Pipeline => _pipeline ??= new MarkdownPipelineBuilder()
                                                             .UseAdvancedExtensions()
                                                             .UseYamlFrontMatter()
                                                             .Build();

    public static bool Compile(Project proj)
    {
        if (!SEH.IO(proj.Info.BuildDirectory, dir =>
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            })) 
            return false;
        
        var total   = 0;
        var success = 0;
        Logger.Log(LogLevel.INFO, "Preprocessing start", proj.Info.Name);
        Parallel.ForEach(EnumerateFiles(new DirectoryInfo(proj.Info.PostDirectory)), file =>
        {
            Logger.Log(LogLevel.INFO, "Preprocessing start", file.FullName);
            Interlocked.Increment(ref total);
            if (Compile(file.FullName, proj))
            {
                Interlocked.Increment(ref success);
                Logger.Log(LogLevel.CMPL, "Success to preprocess", file.FullName);
            }
            else
            {
                Logger.Log(LogLevel.FAIL, "Failed to preprocess", file.FullName);
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
        var dstHtml = Path.ChangeExtension(dst, ".md");

        return SEH.IO((Yaml: dstYaml, Html: dstHtml), args =>
        {
            var dstInfo = new DirectoryInfo(Path.GetDirectoryName(dst)!);
            if (!dstInfo.Exists) dstInfo.Create();

            var txt = File.ReadAllText(file);
            var md  = Markdown.Parse(txt, Pipeline);

            if (md.Descendants<YamlFrontMatterBlock>().FirstOrDefault() is { } yaml)
            {
                var yamlText = txt.Substring(yaml.Span.Start, yaml.Span.Length);
                yamlText = yamlText
                          .Split('\n')
                          .Skip(1)
                          .SkipLast(1)
                          .Aggregate(new StringBuilder(), (builder, str) => builder.AppendLine(str))
                          .ToString();

                using var reader = new StringReader(yamlText);
                if (Yaml.Deserialize<PostFrontMatter?>(reader) is null)
                    throw new InvalidDataException("Invalid post front matter detected.");

                File.WriteAllText(args.Yaml, yamlText, Encoding.UTF8);

                txt = txt.Remove(yaml.Span.Start, yaml.Span.Length);
                File.WriteAllText(args.Html, txt, Encoding.UTF8);
                Logger.Log(LogLevel.CMPL, $"'{file}' -> '{dstHtml}','{dstYaml}'");
            }
            else
            {
                throw new FileNotFoundException("Yaml front-matter not found.");
            }
        });
    }
}