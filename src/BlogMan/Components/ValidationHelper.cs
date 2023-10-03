using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BlogMan.Components;

public static class ValidationHelper
{
    public static IEnumerable<ValidationResult> Validate<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        T>(
        this T obj)
        where T : IValidatableObject
    {
        return obj.Validate(null!);
    }

    public static IEnumerable<ValidationResult> ValidateProperty<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        T>(
        this T obj,
        string prop,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type propT)
        where T : IValidatableObject
    {
        var info = typeof(T).GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
        if (info is null || info.PropertyType != propT)
            throw new InvalidProgramException();

        var list = new List<ValidationResult>();

        var val = info.GetValue(obj);
        foreach (var attr in info.GetCustomAttributes())
            if (attr is ValidationAttribute validation)
            {
                if (validation.IsValid(val)) 
                    continue;
                list.Add(new ValidationResult(validation.FormatErrorMessage(prop), new[] { prop }));
            }

        return list;
    }

    public static void PrintErrors(
        this IEnumerable<ValidationResult> results,
        string                             target,
        LogLevel                           level = LogLevel.FAIL)
    {
        Logger.Log(level,
            $"""
            Invalid model detected.
            {string.Join('\n', results.Select(r => $"{string.Join(", ", r.MemberNames)} : {r.ErrorMessage}"))}
            """, target);
    }
}