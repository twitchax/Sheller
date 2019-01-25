using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The executable type for `sleep`.
    /// </summary>
    public class Sleep : Executable<Sleep>
    {
        /// <summary>
        /// Initializes this instance with the provided shell.
        /// </summary>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <returns>This instance.</returns>
        public override Sleep Initialize(IShell shell)
        {
            return this.Initialize("sleep", shell);
        }
    }
}