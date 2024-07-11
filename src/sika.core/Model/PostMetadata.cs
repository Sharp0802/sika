// Copyright (C)  2023  Yeong-won Seo
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
using sika.core.Model.Abstract;

namespace sika.core.Model;

public class PostMetadata(
    string     lcid,
    string     layout,
    string     title,
    DateTime[] timestamps,
    string[]   topic)
    : ModelBase
{
    internal PostMetadata(RawPostMetadata raw) : this(
        raw.LCID,
        raw.Layout,
        raw.Title,
        raw.Timestamps.Select(DateTime.Parse).ToArray(),
        raw.Topic)
    {
    }

    public string     LCID       { get; set; } = lcid;
    public string     Layout     { get; set; } = layout;
    public string     Title      { get; set; } = title;
    public DateTime[] Timestamps { get; set; } = timestamps;
    public string[]   Topic      { get; set; } = topic;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class RawPostMetadata
    {
        [Obsolete($"parameterless ctor in {nameof(RawPostMetadata)} is only for (de)serialization")]
        public RawPostMetadata()
        {
        }

        public RawPostMetadata(PostMetadata source)
        {
            LCID       = source.LCID;
            Layout     = source.Layout;
            Title      = source.Title;
            Timestamps = source.Timestamps.Select(t => t.ToString("yyyy-MM-dd")).ToArray();
            Topic      = source.Topic;
        }

        public string   LCID       { get; set; } = null!;
        public string   Layout     { get; set; } = null!;
        public string   Title      { get; set; } = null!;
        public string[] Timestamps { get; set; } = null!;
        public string[] Topic      { get; set; } = null!;
    }
}