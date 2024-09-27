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

using System.Collections.Concurrent;
using System.Xml.Linq;
using sika.core.Components.Abstract;
using sika.core.Model;

namespace sika.core.Components;

public class GoogleSitemapLinker(Project project) : ILinker, IDisposable
{
    private readonly ConcurrentBag<XElement> _urls = [];
    private readonly XNamespace              _ns   = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public void Dispose()
    {
        var urlSet = new XElement(_ns + "urlset", _urls.OrderBy(url => url.Element(_ns + "loc")!.Value));
        File.WriteAllText(
            Path.Combine(project.Info.SiteDirectory, "sitemap.xml"), 
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + urlSet.ToString(SaveOptions.OmitDuplicateNamespaces));
    }

    public bool IsSynchronous => false;
    
    public bool CanExecute(PageTree tree)
    {
        return tree.Content is PageLeafData;
    }

    public Task CompileAsync(PageTree tree)
    {
        var loc     = project.Info.RootUri + tree.GetFullPath();
        var lastMod = ((PageLeafData)tree.Content).Metadata.Timestamps.Last();

        var element = new XElement(_ns + "url",
            new XElement(_ns + "loc", new XText(loc)),
            new XElement(_ns + "lastmod", lastMod.ToString("yyyy-MM-dd")));
        _urls.Add(element);

        return Task.CompletedTask;
    }
}