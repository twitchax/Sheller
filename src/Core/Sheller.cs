using System;
using System.Threading;
using Sheller.Implementations;
using Sheller.Implementations.Shells;
using Sheller.Models;

// TODO:
//   * "Abstractrify" the Shell class and make it return the concrete type (e.g., `TShell`).

namespace Sheller
{
    /// <summary>
    /// The entrypoint static class for building shell and executables contexts.
    /// </summary>
    public static class Sheller
    {
        /// <summary>
        /// Creates a new shell instance.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>
        /// <returns>The shell instance.</returns>
        public static GenericShell Shell(string shell) => new GenericShell().Initialize(shell);

        /// <summary>
        /// Creates a new shell instance.
        /// </summary>
        /// <typeparam name="TShell">The type of the shell to instantiate.</typeparam>
        /// <returns>The shell instance.</returns>
        public static TShell Shell<TShell>() where TShell : Shell<TShell>, new() => new TShell().Initialize();
    }
}
