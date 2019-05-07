using System;

namespace Rocket.Libraries.Sessions.Services
{
    internal static class LogUtility
    {
        public static void Debug(string str)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(str, "debug");
        }

        public static void Error(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(str, "error");
        }

        public static void Warn(string str)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Write(str, "warn");
        }

        private static void Write(string str, string level)
        {
            Console.WriteLine($"\tSessionsMiddleware|\t{level.ToUpper()}|\t{str}");
            Console.ResetColor();
        }
    }
}