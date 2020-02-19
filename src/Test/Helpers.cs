using System.Diagnostics.CodeAnalysis;
using Sheller.Models;

[assembly: SuppressMessage("Readability", "RCS1090", Justification = "No need for `Configureawait` in tests.")]

namespace Sheller.Tests
{
    public static class Extensions
    {
        public static IExecutable ToExecutable(this IExecutable exe) => (IExecutable)exe;
    }
}