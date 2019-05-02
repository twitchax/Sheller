using System;
using System.Linq;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The interface for a generic shell.  Note: this assumes the shell is *nixy.
    /// </summary>
    public interface ICmd : IShell<ICmd> {}

    /// <summary>
    /// The shell type for `cmd.exe`.
    /// </summary>
    public class Cmd : Shell<ICmd>, ICmd
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Cmd"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Shell<ICmd> Create() => new Cmd();

        /// <summary>
        /// The <cref see="Cmd"/> constructor.
        /// </summary>
        public Cmd() : base("cmd.exe") {}

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