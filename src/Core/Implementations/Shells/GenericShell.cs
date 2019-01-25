
using System.Linq;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The executable type for a generic shell.  Note: this assumes the shell is *nixy.
    /// </summary>
    public class GenericShell : ShellBase<GenericShell>
    {
        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represents the command argument that should be passed to this shell.</returns>
        public override string GetCommandArgument(string executableCommand)
        {
            var environmentVariables = this.EnvironmentVariables.Aggregate("", (agg, kvp) => agg += $"export {kvp.Key}=\"{kvp.Value.EscapeQuotes()}\";");
            return $"-c \"{environmentVariables}{ executableCommand.EscapeQuotes() }\"";
        }
    }
}