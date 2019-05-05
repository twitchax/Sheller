using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sheller.Implementations.Executables;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The shell interface for `bash`.
    /// </summary>
    public interface IBash : IShell {}

    /// <summary>
    /// The shell type for `bash`.
    /// </summary>
    public class Bash : Shell<IBash>, IBash
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Bash"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Shell<IBash> Create() => new Bash();

        /// <summary>
        /// The <cref see="Bash"/> constructor.
        /// </summary>
        public Bash() : base("bash") {}

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