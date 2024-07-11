// Copyright (C)  2024  Yeong-won Seo
// 
// SIKA is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SIKA is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

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