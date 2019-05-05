using System;
using System.Collections.Generic;
using System.Threading;
using Sheller.Implementations;
using Sheller.Implementations.Shells;
using Sheller.Models;

// TODO:
//   * stdout/stderr to memorystream?

namespace Sheller
{
    /// <summary>
    /// The entrypoint static class for building shell and executables contexts.
    /// </summary>
    public static class Builder
    {
        /// <summary>
        /// Creates a new shell instance.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>
        /// <returns>The shell instance.</returns>
        public static IGenericShell UseShell(string shell) => new GenericShell(shell);

        /// <summary>
        /// Creates a new shell instance.
        /// </summary>
        /// <typeparam name="TShell">The type of the shell to instantiate.</typeparam>
        /// <returns>The shell instance.</returns>
        public static TShell UseShell<TShell>() where TShell : IShell, new() => new TShell();
    }
}
