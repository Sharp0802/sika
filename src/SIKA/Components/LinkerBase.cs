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
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.MediaLinks;
using Markdig.Syntax.Inlines;
using SIKA.Models;
using SIKA.Models.IO;
using SIKA.Models.Posts;
using SIKA.Models.Utilities;

namespace SIKA.Components;

public class LinkerEventArgs : EventArgs
{
    public LinkerEventArgs(
        Project  project,
        PostRoot tree,
        PostLeaf node,
        string   content,
        FileInfo dest)
    {
        Project     = project;
        PostTree    = tree;
        PostNode    = node;
        Content     = content;
        Destination = dest;
    }

    public Project  Project     { get; }
    public PostRoot PostTree    { get; }
    public PostLeaf PostNode    { get; }
    public string   Content     { get; }
    public FileInfo Destination { get; }
}

public abstract class LinkerBase : IDisposable
{
    protected LinkerBase(Project project)
    {
        Project = project;

        var fst = new FileSystemTree(
            new DirectoryInfo(project.Info.BuildDirectory),
            fi => fi.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
        );
        Tree = PostTree.Burn(fst);

        ManglingMap = ((PostTree)Tree).Traverse().OfType<PostLeaf>().ToDictionary(
            leaf => Path.GetRelativePath(project.Info.BuildDirectory, leaf.Info.FullName),
            leaf => leaf.GetHRef()
        );

        Pipeline = new MarkdownPipelineBuilder()
                  .UseBootstrap()
                  .UseCitations()
                  .UseAdvancedExtensions()
                  .UseDiagrams()
                  .UseFigures()
                  .UseFooters()
                  .UseFootnotes()
                  .UseGlobalization()
                  .UseMathematics()
                  .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                  .UseAutoLinks(new AutoLinkOptions { OpenInNewWindow = true, UseHttpsForWWWLinks = true })
                  .UseDefinitionLists()
                  .UseEmphasisExtras()
                  .UseGenericAttributes()
                  .UseGridTables()
                  .UseListExtras()
                  .UseMediaLinks(new MediaOptions { AddControlsProperty = true })
                  .UsePipeTables()
                  .UsePragmaLines()
                  .UseReferralLinks()
                  .UseSmartyPants()
                  .UseTaskLists()
                  .UseEmojiAndSmiley()
                  .UseYamlFrontMatter()
                  .UseUrlRewriter(OnVisitLink)
                  .Build();
    }

    protected Project Project { get; }

    private PostRoot                   Tree        { get; }
    private Dictionary<string, string> ManglingMap { get; }
    private MarkdownPipeline           Pipeline    { get; }

    private string OnVisitLink(LinkInline link)
    {
        const string prefix = "sika://";
        var          url    = link.Url ?? string.Empty;
        return url.StartsWith(prefix) ? ManglingMap[url[prefix.Length..]] : url;
    }

    protected abstract bool Initialize();

    protected abstract bool Link(LinkerEventArgs args);

    protected abstract void CleanUp();

    public bool Run()
    {
        var conflicts = Tree.Validate().ToArray();
        if (conflicts.Any())
        {
            Logger.Log(LogLevel.FAIL, "conflicts detected between posts");
            foreach (var conflict in conflicts)
            {
                var lst = from fsi in conflict.Conflicts select $"\n-{fsi.Info.FullName}";
                Logger.Log(
                    LogLevel.FAIL,
                    $"{conflict.Message}:{lst.Aggregate(new StringBuilder(), (b, c) => b.Append(c))}");
            }

            return false;
        }

        var failed = false;

        Logger.Log(LogLevel.INFO, "start local initializing");
        if (!Initialize())
        {
            Logger.Log(LogLevel.FAIL, "failed local initializing");
            failed = true;
        }

        if (failed)
            return false;
        Logger.Log(LogLevel.CMPL, "complete local initializing");


        Parallel.ForEach(((PostTree)Tree).Traverse().OfType<PostLeaf>(), node =>
        {
            var id   = node.GetIdentifier();
            var href = node.GetHRef();
            if (href.StartsWith('/'))
                href = href[1..];

            Logger.Log(LogLevel.INFO, $"start building: '{id}'");

            var succeeded = SEH.IO(id, _ =>
            {
                // initialize event-args
                var fd = new FileInfo(Path.Combine(Project.Info.SiteDirectory, href));
                var md = Markdown.ToHtml(File.ReadAllText(node.Info.FullName), Pipeline);

                var args = new LinkerEventArgs(Project, Tree, node, md, fd);

                // run link event
                if (!Link(args))
                    throw new InvalidOperationException("failed to complete link process");
            });

            // log result
            if (!succeeded)
            {
                Logger.Log(LogLevel.FAIL, $"'{id}' -> <failed>");
                failed = true;
            }
            else
            {
                Logger.Log(LogLevel.CMPL, $"'{id}' -> '{href}'");
            }
        });

        return !failed;
    }

    public void Dispose()
    {
        CleanUp();
        GC.SuppressFinalize(this);
    }
}