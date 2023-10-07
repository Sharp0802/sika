//     Copyright (C) 2023  Yeong-won Seo
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

using System.Collections.Concurrent;
using System.Text;
using Markdig;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using SIKA.Models;

namespace SIKA.Components;

public sealed class RazorTemplateLinker : LinkerBase
{
    private readonly ConcurrentDictionary<string, string> _layoutMap = new();
    private readonly ThreadLocal<MarkdownPipeline>        _pipeline  = new();

    public RazorTemplateLinker(Project project) : base(project)
    {
        var config = new TemplateServiceConfiguration();
        RazorService = RazorEngineService.Create(config);
    }

    private IRazorEngineService RazorService { get; }

    protected override void CleanUp()
    {
        _pipeline.Dispose();
    }

    private string GetLayout(string key)
    {
        return _layoutMap.GetOrAdd(key, p =>
        {
            p = Path.ChangeExtension(p, ".razor");
            return File.ReadAllText(p.ToLowerInvariant().Equals("default.razor", StringComparison.Ordinal)
                ? Path.Combine(AppContext.BaseDirectory,     "Resources/post.razor")
                : Path.Combine(Project.Info.LayoutDirectory, p));
        });
    }


    protected override bool Initialize()
    {
        return SEH.IO(Path.Combine(AppContext.BaseDirectory, "wwwroot"), dir =>
        {
            var dst = new DirectoryInfo(Project.Info.SiteDirectory);
            if (dst.Exists)
                dst.Create();
            new DirectoryInfo(dir).CopyTo(dst);
        });
    }

    protected override bool Link(LinkerEventArgs args)
    {
        var metadata = args.PostNode.Metadata;
        if (metadata is null)
        {
            Logger.Log(LogLevel.FAIL, "Metadata not found.");
            return false;
        }
        
        var errors = metadata.Validate().ToArray();
        if (errors.Length != 0)
        {
            Logger.Log(LogLevel.FAIL, "Invalid metadata detected.");
            errors.PrintErrors(args.PostNode.GetIdentifier());
            return false;
        }

        var html = RazorService.RunCompile(
            GetLayout(metadata.Layout),
            Guid.NewGuid().ToString(),
            typeof(TemplateModel),
            new TemplateModel(args.Project, metadata, args.PostTree, args.Content));

        using var fd = args.Destination.Open(FileMode.Create);
        using var sw = new StreamWriter(fd, Encoding.UTF8);
        sw.Write(html);
        
        return true;
    }
}