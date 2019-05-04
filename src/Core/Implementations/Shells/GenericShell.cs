
using System.Linq;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The interface for a generic shell.  Note: this assumes the shell is *nixy.
    /// </summary>
    public interface IGenericShell : IShell {}

    /// <summary>
    /// The type for a generic shell.  Note: this assumes the shell is *nixy.
    /// </summary>
    public class GenericShell : Shell<IGenericShell>, IGenericShell
    {
        /// <summary>
        /// Creates a new instance of the <see cref="GenericShell"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Shell<IGenericShell> Create() => new GenericShell(null);

        /// <summary>
        /// <cref see="GenericShell"/> is a special case that cannot be initialized without a shell name.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>       
        public GenericShell(string shell) : base(shell) {}

        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represents the command argument that should be passed to this shell.</returns>
        public override string GetCommandArgument(string executableCommand)
        {
            var environmentVariables = this.EnvironmentVariables.Aggregate("", (agg, kvp) => agg += $"export {kvp.Key}=\"{kvp.Value.EscapeQuotes()}\"; ");
            return $"-c \"{environmentVariables}{ executableCommand.EscapeQuotes() }\";";
        }
    }
}