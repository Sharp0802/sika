using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

namespace SIKA.Components;

public static class SEH
{
    public static bool IO<T>(T target, Action<T> io)
    {
        try
        {
            io.Invoke(target);
            return true;
        }
        catch (IOException e)
        {
            Logger.Log(LogLevel.FAIL, $"IO operation failed: {e.Message}", target?.ToString() ?? "<null>");
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException)
        {
            Logger.Log(LogLevel.FAIL, $"Unauthorized IO operation: {e.Message}", target?.ToString() ?? "<null>");
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.FAIL, e, target?.ToString() ?? "<null>");
        }

        return false;
    }

    public static bool IO<TSource, TReturn>(TSource target, Func<TSource, TReturn?> io, [NotNullWhen(true)] out TReturn? ret)
        where TReturn : class
    {
        try
        {
            ret = io.Invoke(target);
            return ret is not null;
        }
        catch (IOException)
        {
            Logger.Log(LogLevel.FAIL, "IO operation failed", target?.ToString() ?? "<null>");
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException)
        {
            Logger.Log(LogLevel.FAIL, "Unauthorized IO operation", target?.ToString() ?? "<null>");
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.FAIL, e, target?.ToString() ?? "<null>");
        }

        Unsafe.SkipInit(out ret);
        return false;
    }
}