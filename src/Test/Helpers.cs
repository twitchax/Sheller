using Sheller.Models;

namespace Sheller.Tests
{
    public static class Extensions
    {
        public static IExecutable ToExecutable(this IExecutable exe) => (IExecutable)exe;
    }
}