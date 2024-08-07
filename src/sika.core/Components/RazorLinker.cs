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

using System.Collections.Frozen;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.MediaLinks;
using Markdig.Syntax.Inlines;
using RazorLight;
using sika.core.Components.Abstract;
using sika.core.Model;

namespace sika.core.Components;

public class RazorLinker : ILinker
{
    private readonly FrozenDictionary<string, string> _pageManglings;
    private readonly MarkdownPipeline                 _pipeline;
    private readonly Project                          _project;
    private readonly RazorLightEngine                 _razorEngine;

    public RazorLinker(PageTree tree, Project project)
    {
        _project = project;

        _pipeline = new MarkdownPipelineBuilder()
            // EXTENSIONS
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .UseAdvancedExtensions()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseAutoLinks(new AutoLinkOptions
            {
                OpenInNewWindow     = true,
                UseHttpsForWWWLinks = true
            })
            .UseMediaLinks(new MediaOptions
            {
                AddControlsProperty = true
            })
            .UseYamlFrontMatter()
            // MODULES
            .UseUrlRewriter(UrlRewriter)
            .Build();

        _pageManglings = tree.GetManglingMap(_project.Info.RootUri).ToFrozenDictionary();

        _razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(_project.Info.LayoutDirectory)
            .UseMemoryCachingProvider()
            .Build();
    }

    public bool IsSynchronous => false;

    public bool CanExecute(PageTree tree)
    {
        return tree.Content is PageLeafData;
    }

    public async Task CompileAsync(PageTree tree)
    {
        var data = (PageLeafData)tree.Content;
        data.Content = Markdown.ToHtml(data.Content, _pipeline);
        
        var result = await _razorEngine.CompileRenderAsync(
            data.Metadata.Layout,
            new TemplateModel(_project, tree));
        data.Content = result;
    }

    private string UrlRewriter(LinkInline arg)
    {
        if (arg.Url is null)
            return string.Empty;

        const string prefix = "sika://";

        if (!arg.Url.StartsWith(prefix))
            return arg.Url;

        var target = arg.Url[prefix.Length..];
        if (!_pageManglings.TryGetValue(target, out var url))
            throw new KeyNotFoundException($"page '{target}' not found");

        return Path.ChangeExtension(url, ".html");
    }
}