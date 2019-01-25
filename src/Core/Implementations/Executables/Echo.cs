using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The executable type for `echo`.
    /// </summary>
    public class Echo : Executable<Echo, string>
    {
        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <returns>A task which results in a <see cref="string"/> (i.e., the result of the execution).</returns>
        public override Task<string> ExecuteAsync() => this.ExecuteAsync(cr => cr.StandardOutput.Trim());

        /// <summary>
        /// Initializes an <see cref="Echo"/> instance with the provided shell.
        /// </summary>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <returns>This instance.</returns>
        public override Echo Initialize(IShell shell)
        {
            return this.Initialize("echo", shell);
        }
    }
}