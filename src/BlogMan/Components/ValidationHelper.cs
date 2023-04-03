using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BlogMan.Components;

public static class ValidationHelper
{
    [SuppressMessage("Trimming",
        "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
    public static IEnumerable<ValidationResult> Validate<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        T>(this T obj)
        where T : IValidatableObject
    {
        var list = new List<ValidationResult>();
        Validator.TryValidateObject(obj, new ValidationContext(obj, null, null), list, true);
        return list;
    }

    public static IEnumerable<ValidationResult> ValidateProperty<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        T>(this T obj, string prop)
        where T : IValidatableObject
    {
        var val = obj
            .GetType()
            .GetProperty(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(obj);

        var list = new List<ValidationResult>();
        Validator.TryValidateProperty(val, new ValidationContext(obj, null, null) { MemberName = prop }, list);
        return list;
    }

    public static void PrintErrors(this IEnumerable<ValidationResult> results, string target, LogLevel level = LogLevel.FAIL)
    {
        Logger.Log(level,
            $"""
            Invalid model detected.
            {string.Join('\n', results.Select(r => $"{string.Join(", ", r.MemberNames)} : {r.ErrorMessage}"))}
            """, target);
    }
}