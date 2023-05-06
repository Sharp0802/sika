using BlogMan.Models;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.MediaLinks;
using Markdig.Syntax.Inlines;

namespace BlogMan.Components;

public class LinkerEventArgs : EventArgs
{
    public LinkerEventArgs(Project project, PostTree tree, PostTreeNode node, string content, FileInfo dest)
    {
        Project     = project;
        PostTree    = tree;
        PostNode    = node;
        Content     = content;
        Destination = dest;
    }

    public Project      Project     { get; }
    public PostTree     PostTree    { get; }
    public PostTreeNode PostNode    { get; }
    public string       Content     { get; }
    public FileInfo     Destination { get; }
}

public abstract class LinkerBase
{
    protected LinkerBase(Project project)
    {
        Project = project;

        var nodes = new DirectoryInfo(project.Info.BuildDirectory)
                   .EnumerateFileSystemInfos()
                   .Where(info => (info.Attributes & FileAttributes.Directory) != 0 ||
                                  info.Extension.ToLowerInvariant().Equals(".md", StringComparison.Ordinal))
                   .Select(info => new PostTreeNode(info, null))
                   .ToArray();

        Tree = new PostTree(nodes);

        ManglingMap = nodes.ToDictionary(
            node => Path.GetRelativePath(project.Info.BuildDirectory, node.FileRecord.FullName),
            node => node.HtmlIdentifier);

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

    private PostTree                   Tree        { get; }
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
        var gFailed = !SEH.IO(Project.Info.SiteDirectory, dir =>
        {
            var info = new DirectoryInfo(dir);
            if (info.Exists)
                info.Delete(true);
            info.Create();
        });


        Logger.Log(LogLevel.INFO, "start global initializing");
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


        Parallel.ForEach(Tree.GetAllFile(), node =>
        {
            Logger.Log(LogLevel.INFO, $"start building: '{node.Identifier}'");

            var succeeded = SEH.IO(node.Identifier, _ =>
            {
                // initialize event-args
                var args = new LinkerEventArgs(
                    Project,
                    Tree,
                    node,
                    Markdown.ToHtml(File.ReadAllText(node.FileRecord.FullName), Pipeline),
                    new FileInfo(Path.Combine(Project.Info.SiteDirectory, node.HtmlIdentifier)));

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
                Logger.Log(LogLevel.FAIL, $"'{node.Identifier}' -> <failed>");
                gFailed = true;
            }
            else
            {
                Logger.Log(LogLevel.CMPL, $"'{node.Identifier}' -> '{node.HtmlIdentifier}'");
            }
        });

        return !gFailed;
    }
}