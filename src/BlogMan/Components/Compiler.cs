using BlogMan.Models;
using Markdig;

namespace BlogMan.Components;

public class Compiler
{
    [field: ThreadStatic]
    private static MarkdownPipeline Pipeline { get; } = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build();

    private static void Compile(string file, Project proj)
    {

    }
}