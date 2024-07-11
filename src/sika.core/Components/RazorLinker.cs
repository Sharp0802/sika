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
    private readonly Project                          _project;
    private readonly MarkdownPipeline                 _pipeline;
    private readonly FrozenDictionary<string, string> _pageManglings;
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

        return url;
    }

    public async Task CompileAsync(PageTree tree)
    {
        var data = (PageLeafData)tree.Content;

        var result = await _razorEngine.CompileRenderAsync(data.Metadata.Layout, new
        {
            Header   = new HeaderInfo(data.Metadata.Title, data.Metadata.Topic, data.Metadata.Timestamps),
            PostTree = tree.GetRoot(),
            Html     = Markdown.ToHtml(data.Content, _pipeline),

            // FOR BACK-COMPATIBILITY
            Layout   = data.Metadata.Layout,
            Metadata = data.Metadata,
            Profile  = _project.Profile,
            Contacts = _project.Contacts
        });
        data.Content = result;
    }
}