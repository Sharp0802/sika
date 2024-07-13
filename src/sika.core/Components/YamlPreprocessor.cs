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

using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using sika.core.Components.Abstract;
using sika.core.Model;
using sika.core.Text;
using YamlDotNet.Core;

namespace sika.core.Components;

public class YamlPreprocessor : ILinker
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build();

    public Task CompileAsync(PageTree tree)
    {
        var leafData = (PageLeafData)tree.Content;
        
        var md = Markdown.Parse(leafData.Content, _pipeline);
        if (md.Descendants<YamlFrontMatterBlock>().SingleOrDefault() is not { } yaml)
            throw new InvalidDataException("Yaml metadata not found -or- duplicated");

        // Skip separator ('---')
        var yamlText = string.Join('\n', leafData.Content
            .Substring(yaml.Span.Start, yaml.Span.Length)
            .Split('\n')
            .Skip(1)
            .SkipLast(1));
        
        PostMetadata.RawPostMetadata rawMetadata;
        try
        {
            rawMetadata = new Yaml().Deserialize<PostMetadata.RawPostMetadata>(yamlText);
        }
        catch (YamlException e)
        {
            throw new FileLoadException($"Couldn't deserialize yaml:\n--- DUMP ---{yamlText}\n--- END ---", e);
        }

        leafData.Content  = leafData.Content.Remove(yaml.Span.Start, yaml.Span.Length);
        leafData.Metadata = new PostMetadata(rawMetadata);

        return Task.CompletedTask;
    }
}