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
using System.Xml;
using System.Xml.Linq;
using SIKA.Models;

namespace SIKA.Components;

public sealed class GoogleSitemapLinker : LinkerBase
{
    private static XNamespace Xmlns => "http://www.sitemaps.org/schemas/sitemap/0.9";

    private readonly object    _sync = new();
    private          XElement? _root;

    public GoogleSitemapLinker(Project project) : base(project)
    {
    }

    protected override bool Initialize()
    {
        _root = new XElement(Xmlns.GetName("urlset"));
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
                    Xmlns.GetName("url"),
                    new XElement(Xmlns.GetName("loc"),     uri),
                    new XElement(Xmlns.GetName("lastmod"), mod)
                )
            );
        }

        return true;
    }

    protected override void CleanUp()
    {
        var xml = new XDocument(new XDeclaration("1.0", "UTF-8", null));
        xml.Add(_root);

        var dst = Path.Combine(Project.Info.SiteDirectory, "sitemap.xml");

        using var stream = new FileStream(dst, FileMode.Create, FileAccess.Write);
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8, 
            Indent = true,
            IndentChars = "\t"
        });
        
        xml.WriteTo(writer);
    }
}