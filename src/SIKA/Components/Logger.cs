//     Copyright (C) 2023  Yeong-won Seo
// 
//     SIKA is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     SIKA is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

namespace SIKA.Components;

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
        ConsoleColor.DarkRed,
        ConsoleColor.Yellow,
        ConsoleColor.Gray,
        ConsoleColor.Green
    };

    private static readonly string[] Headers =
    {
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