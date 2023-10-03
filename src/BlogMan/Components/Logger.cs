namespace BlogMan.Components;

public enum LogLevel
{
    FAIL,
    WARN,
    INFO,
    CMPL
}

public static class Logger
{
    private static readonly object LockHandle = new();

    private static readonly ConsoleColor[] Colors =
    {
        ConsoleColor.Red,
        ConsoleColor.DarkRed,
        ConsoleColor.Yellow,
        ConsoleColor.Gray,
        ConsoleColor.Green
    };

    private static readonly string[] Headers =
    {
        "crit",
        "fail",
        "warn",
        "info",
        "cmpl"
    };

    public static void Log(LogLevel lv, string msg)
    {
        lock (LockHandle)
        {
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

    public static void Log(LogLevel lv, Exception ex, string target)
    {
        lock (LockHandle)
        {
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