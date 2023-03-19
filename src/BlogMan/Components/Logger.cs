namespace BlogMan.Components;

public enum LogLevel
{
    CRIT,
    FAIL,
    WARN,
    INFO,
    CMPL
}

public class Logger
{
    private static object LockHandle = new();

    private static ConsoleColor[] Colors = new[]
    {
        ConsoleColor.Red,
        ConsoleColor.DarkRed,
        ConsoleColor.Yellow,
        ConsoleColor.Gray,
        ConsoleColor.Green
    };

    private static string[] Headers = new[]
    {
        "crit",
        "fail",
        "warn",
        "info",
        "cmpl",
    };

    public static void Log(LogLevel lv, string msg)
    {
        lock (LockHandle)
        {
            Console.Write("blogman: ");
            var pre = Console.ForegroundColor;
            Console.ForegroundColor = Colors[(int)lv];
            Console.Write(Headers[(int)lv]);
            Console.ForegroundColor = pre;
            Console.Write(": ");
            Console.WriteLine(msg);
        }
    }

    public static void Log(LogLevel lv, string msg, string target)
    {
        Log(lv, $"{target}: {msg}");
    }

    public static void Log(LogLevel lv, Exception ex)
    {
        lock (LockHandle)
        {
            Console.Write("blogman: ");
            var pre = Console.ForegroundColor;
            Console.ForegroundColor = Colors[(int)lv];
            Console.Write(Headers[(int)lv]);
            Console.ForegroundColor = pre;
            Console.WriteLine(": === EXCEPTION ===");
            Console.WriteLine(ex.ToString());
        }
    }

    public static void Log(LogLevel lv, Exception ex, string target)
    {
        lock (LockHandle)
        {
            Console.Write("blogman: ");
            var pre = Console.ForegroundColor;
            Console.ForegroundColor = Colors[(int)lv];
            Console.Write(Headers[(int)lv]);
            Console.ForegroundColor = pre;
            Console.Write(": ");
            Console.Write(target);
            Console.WriteLine(": === EXCEPTION ===");
            Console.WriteLine(ex.ToString());
        }
    }
}