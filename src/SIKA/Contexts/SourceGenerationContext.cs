using System.Text.Json.Serialization;
using SIKA.Models;

namespace SIKA.Contexts;

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(Contacts))]
[JsonSerializable(typeof(LinkReference))]
[JsonSerializable(typeof(ProfileInfo))]
[JsonSerializable(typeof(ProjectInfo))]
[JsonSerializable(typeof(string))]
public partial class SourceGenerationContext : JsonSerializerContext;
