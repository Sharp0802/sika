using System.Text;
using BlogMan.Models;
using BlogMan.Models.IO;
using BlogMan.Models.Posts;
using BlogMan.Models.Utilities;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.MediaLinks;
using Markdig.Syntax.Inlines;

namespace BlogMan.Components;

public class LinkerEventArgs : EventArgs
{
    public LinkerEventArgs(Project project, PostRoot tree, PostLeaf node, string content, FileInfo dest)
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
        const string prefix = "ref::";
        var          url    = link.UnescapedUrl.Text;
        return url.StartsWith(prefix) ? ManglingMap[url[prefix.Length..]] : url;
    }

    protected abstract bool Initialize();

    protected abstract bool Link(LinkerEventArgs args);

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

        Logger.Log(LogLevel.INFO, "start global initializing");
        
        var gFailed = !SEH.IO(Project.Info.SiteDirectory, dir =>
        {
            var info = new DirectoryInfo(dir);
            if (info.Exists)
                info.Delete(true);
            info.Create();
        });
        if (gFailed)
        {
            Logger.Log(LogLevel.FAIL, "failed global initializing");
            return false;
        }

        Logger.Log(LogLevel.CMPL, "complete global initializing");

        Logger.Log(LogLevel.INFO, "start local initializing");
        if (!Initialize())
        {
            Logger.Log(LogLevel.FAIL, "failed local initializing");
            gFailed = true;
        }

        if (gFailed)
            return false;
        Logger.Log(LogLevel.CMPL, "complete local initializing");


        Parallel.ForEach(((PostTree)Tree).Traverse().OfType<PostLeaf>(), node =>
        {
            var id   = node.GetIdentifier();
            var href = node.GetHRef();

            Logger.Log(LogLevel.INFO, $"start building: '{id}'");

            var succeeded = SEH.IO(id, _ =>
            {
                // initialize event-args
                var args = new LinkerEventArgs(
                    Project,
                    Tree,
                    node,
                    Markdown.ToHtml(File.ReadAllText(node.Info.FullName), Pipeline),
                    new FileInfo(Path.Combine(Project.Info.SiteDirectory, href)));

                // ensure file
                if (!args.Destination.Exists)
                    args.Destination.Create();

                // run link event
                if (!Link(args))
                    throw new InvalidOperationException("failed to complete link process");
            });

            // log result
            if (!succeeded)
            {
                Logger.Log(LogLevel.FAIL, $"'{id}' -> <failed>");
                gFailed = true;
            }
            else
            {
                Logger.Log(LogLevel.CMPL, $"'{id}' -> '{href}'");
            }
        });

        return !gFailed;
    }

    public abstract void Dispose();
}