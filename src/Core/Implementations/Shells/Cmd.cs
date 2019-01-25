using System;
using System.Linq;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The shell type for `cmd.exe`.
    /// </summary>
    public class Cmd : Shell<Cmd>
    {
        /// <summary>
        /// Instantiates a <see cref="Bash"/> instance.
        /// </summary>
        /// <returns>The instance.</returns>
        public override Cmd Initialize() => Initialize("cmd.exe");

        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represents the command argument that should be passed to this shell.</returns>
        public override string GetCommandArgument(string executableCommand)
        {
            // TODO: this may not work?  But...ehhh.
            var environmentVariables = this.EnvironmentVariables.Aggregate("", (agg, kvp) => agg += $"set {kvp.Key}=\"{kvp.Value.EscapeQuotes()}\" && ");
            return $"/c \"{environmentVariables}{ executableCommand.EscapeQuotes() }\"";
        }
    }
}