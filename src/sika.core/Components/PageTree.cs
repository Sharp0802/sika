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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using sika.core.Components.Abstract;
using sika.core.Model;

namespace sika.core.Components;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public class PageTree
{
    public PageTree()
    {
        Content = new PageTreeData("");
        Parent  = null;
    }

    private PageTree(PageTreeData data, PageTree? parent)
    {
        Content = data;
        Parent  = parent;
    }

    public PageTreeData   Content  { get; }
    public PageTree?      Parent   { get; }
    public List<PageTree> Children { get; } = [];

    private IEnumerable<PageTree> Traverse()
    {
        var queue = new Queue<PageTree>();

        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in current.Children)
                queue.Enqueue(child);
        }
    }

    internal string GetFullPath()
    {
        var stack = new Stack<string>();
        var tree  = this;
        do
        {
            stack.Push(tree.Content.Name);
            tree = tree.Parent;
        } while (tree?.Parent != null);

        return string.Join('/', stack);
    }

    internal PageTree GetRoot()
    {
        var current = this;
        while (current.Parent is not null)
            current = current.Parent;
        return current;
    }

    internal Dictionary<string, string> GetManglingMap(string root)
    {
        return Traverse()
            .Where(tree => tree.Content is PageLeafData)
            .ToDictionary(tree => tree.GetFullPath(), tree => root + tree.GetFullPath());
    }

    private XElement GetHtmlInternal(Project project)
    {
        if (Content is PageLeafData leaf)
        {
            return new XElement("li",
                new XElement("a",
                    new XAttribute("href", project.Info.RootUri + Path.ChangeExtension(GetFullPath(), ".html")),
                    new XText(leaf.Metadata.Title)));
        }

        return new XElement("li",
            new XElement("span",
                new XAttribute("class", "caret"),
                new XText(Content.Name)),
            new XElement("ul",
                Children
                    .OrderBy(tree => tree.Content.Name)
                    .Select(child => child.GetHtmlInternal(project) as XObject)
                    .Prepend(new XAttribute("class", "nested"))));
    }

    public string GetHtml(Project project)
    {
        return Children
            .Where(tree => !tree.Content.Name.Equals("404.md"))
            .OrderBy(tree => tree.Content.Name)
            .Select(child => child.GetHtmlInternal(project).ToString(SaveOptions.OmitDuplicateNamespaces))
            .Aggregate(new StringBuilder(), (builder, s) => builder.AppendLine(s))
            .ToString();
    }
    
    public void Initialize(DirectoryInfo source)
    {
        foreach (var fs in source.EnumerateFileSystemInfos())
        {
            PageTree tree = null!;
            switch (fs)
            {
                case DirectoryInfo dir:
                    tree = new PageTree(new PageTreeData(dir.Name), this);
                    tree.Initialize(dir);
                    break;
                case FileInfo file:
                    tree = new PageTree(new PageLeafData(file.Name, null!, File.ReadAllText(file.FullName)), this);
                    break;
            }

            Children.Add(tree);
        }
    }

    public async Task<bool> LinkAsync(ILinker linker)
    {
        var lockHandle = new object();
        var success    = true;
        
        var array = Traverse().Where(tree => tree.Content is PageLeafData).ToArray();
        await Parallel.ForAsync(0, array.Length, async (i, _) =>
        {
            var fullname = array[i].GetFullPath();
            Console.WriteLine($"  [{i + 1}/{array.Length}] {fullname}");
            try
            {
                await linker.CompileAsync(array[i]);
            }
            catch (Exception e)
            {
                lock (lockHandle) 
                    Console.Error.WriteLine($"{fullname}: {e}");
                success = false;
            }
        });
        
        (linker as IDisposable)?.Dispose();

        return success;
    }
}

public record PageTreeData(string Name);

public record PageLeafData(
    string       Name,
    PostMetadata Metadata,
    string       Content)
    : PageTreeData(Name)
{
    public string       Content  { get; set; } = Content;
    public PostMetadata Metadata { get; set; } = Metadata;
}