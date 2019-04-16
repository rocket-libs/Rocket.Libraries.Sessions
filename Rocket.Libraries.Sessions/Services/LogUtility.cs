using System;

namespace Rocket.Libraries.Sessions.Services
{
    internal static class LogUtility
    {
        public static void Debug(string str)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(str);
        }

        public static void Error(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(str);
        }

        private static void Write(string str)
        {
            Console.WriteLine($"\tSessionsMiddleware: {str}");
            Console.ResetColor();
        }
    }
}