using System.Linq;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The shell type for `bash`.
    /// </summary>
    public class Bash : Shell<Bash>
    {
        /// <summary>
        /// Instantiates a <see cref="Bash"/> instance.
        /// </summary>
        /// <returns>The instance.</returns>
        public override Bash Initialize() => Initialize("bash");

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