using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using sika.core.Components.Abstract;
using sika.core.Model;
using sika.core.Text;

namespace sika.core.Components;

public class YamlPreprocessor : IPreprocessor
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build();

    public async Task<PageLeafData> PreprocessAsync(FileInfo file)
    {
        string txt;
        await using (var fs = file.OpenRead())
        using (var sr = new StreamReader(fs, Encoding.UTF8))
            txt = await sr.ReadToEndAsync();

        var md = Markdown.Parse(txt, _pipeline);
        if (md.Descendants<YamlFrontMatterBlock>().SingleOrDefault() is not { } yaml)
            throw new InvalidDataException("Yaml metadata not found -or- duplicated");

        // Skip separator ('---')
        var yamlText = string.Join('\n', txt
            .Substring(yaml.Span.Start, yaml.Span.Length)
            .Split('\n')
            .Skip(1)
            .SkipLast(1));

        if (new Yaml().Deserialize<PostMetadata.RawPostMetadata>(yamlText) is not { } rawMetadata)
            throw new FileLoadException("metadata cannot be serialized");

        var content = txt.Remove(yaml.Span.Start, yaml.Span.Length);

        return new PageLeafData(file.Name, new PostMetadata(rawMetadata), content);
    }
}