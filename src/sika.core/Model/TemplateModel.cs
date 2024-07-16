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

using Microsoft.AspNetCore.Mvc.RazorPages;
using sika.core.Components;

namespace sika.core.Model;

public class TemplateModel(Project project, PageTree tree) : PageModel
{
    public Project      Project  { get; } = project;
    public string       PostTree { get; } = tree.GetRoot().GetHtml(project);
    public PageLeafData Data     { get; } = (PageLeafData)tree.Content;
}