using System.Text;
using System.Xml;
using System.Xml.Linq;
using BlogMan.Models;

namespace BlogMan.Components;

public sealed class GoogleSitemapLinker : LinkerBase
{
    private readonly object _sync = new();
    private XElement? _root;
    
    public GoogleSitemapLinker(Project project) : base(project)
    {
    }

    protected override bool Initialize()
    {
        _root = new XElement(
            "urlset",
            new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9")
        );
        return true;
    }

    protected override bool Link(LinkerEventArgs args)
    {
        var href = args.PostNode.GetHRef();
        if (href.StartsWith('/'))
            href = href[1..];
        
        var uri = new Uri(new Uri(Project.Info.RootUri), href);
        
        var mod = args.PostNode.Metadata?.Timestamps.Max();
        if (mod is null)
            return false;
        
        lock (_sync)
        {
            _root!.Add(new XElement(
                "url",
                    new XElement("loc", uri),
                    new XElement("lastmod", mod)
                )
            );
        }

        return true;
    }

    public override void Dispose()
    {
        var xml = new XDocument(new XDeclaration("1.0", "UTF-8", null));
        xml.Add(_root);
        
        var dst = Path.Combine(Project.Info.SiteDirectory, "sitemap.xml");

        using var writer = new XmlTextWriter(dst, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 4;
        
        xml.Save(writer);
    }
}