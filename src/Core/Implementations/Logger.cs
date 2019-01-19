

using System;
using Sheller.Models;

namespace Sheller.Implementations
{
    public class Logger : ILogger
    {
        private void LogWithColor(string message, ConsoleColor? color = null)
        {
            Console.ForegroundColor = color ?? Console.ForegroundColor;
            Console.WriteLine($"{message}");
            Console.ResetColor();
        }

        public void Log(string message) => LogWithColor(message);
        public void LogInfo(string message) => LogWithColor(message, ConsoleColor.Cyan);
        public void LogWarning(string message) => LogWithColor(message, ConsoleColor.Yellow);
        public void LogError(string message) => LogWithColor(message, ConsoleColor.Red);
    }
}