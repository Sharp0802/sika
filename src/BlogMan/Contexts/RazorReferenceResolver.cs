using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BlogMan.Models;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;

namespace BlogMan.Contexts;

public class RazorReferenceResolver : IReferenceResolver
{
    private static IEnumerable<Assembly> GetAllRequired(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        Type type)
    {
        var set = new HashSet<Assembly>();
        GetAllRequired(type, set);
        return set;
    }

    private static void GetAllRequired(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        Type type,
        HashSet<Assembly> asm)
    {
        if (!asm.Add(type.Assembly))
            return;
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
#pragma warning disable IL2072
            GetAllRequired(prop.PropertyType, asm);
#pragma warning restore IL2072
    }

    private static IEnumerable<Assembly> GetAllRequired()
    {
#pragma warning disable IL2026
        return typeof(RazorReferenceResolver)
              .Assembly
              .GetForwardedTypes()
#pragma warning restore IL2026
              .Where(t => "BlogMan.Models".Equals(t.Namespace, StringComparison.Ordinal))
              .SelectMany(GetAllRequired)
              .Distinct();
    }

    public IEnumerable<CompilerReference> GetReferences(
        TypeContext                    context,
        IEnumerable<CompilerReference> includeAssemblies)
    {
        var asm = GetAllRequired(typeof(TemplateModel));
        foreach (var item in asm)
        {
            File.AppendAllText("log.txt", item.FullName + '\n');
            yield return CompilerReference.From(item);
        }
    }
}