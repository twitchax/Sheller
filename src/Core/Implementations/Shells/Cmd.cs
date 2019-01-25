
using System;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The shell type for `cmd.exe`.
    /// </summary>
    public class Cmd : Shell
    {
        /// <summary>
        /// Instantiates a <see cref="Cmd"/> instance.
        /// </summary>
        /// <returns>The instance.</returns>
        public Cmd() : base("cmd.exe")
        {
            throw new NotImplementedException();
            // TODO: Fix the execute command, etc.
        }
    }
}