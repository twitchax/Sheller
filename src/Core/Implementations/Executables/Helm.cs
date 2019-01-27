using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The executable type for `helm`.
    /// </summary>
    public class Helm : Executable<Helm>
    {
        /// <summary>
        /// Initializes an <see cref="Helm"/> instance with the provided shell.
        /// </summary>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <returns>This instance.</returns>
        public override Helm Initialize(IShell shell) => this.Initialize("helm", shell);

        /// <summary>
        /// Adds a kubeconfig argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.  It is not required, but convention
        /// would dictate that this method be called before arguments.
        /// </remarks>
        /// <param name="configPath"></param>
        /// <returns>A `new` instance of <see cref="Helm"/> with the kubeconfig passed to this call.</returns>
        public Helm WithKubeConfig(string configPath)
        {
            if(this.State.TryGetValue("hasKubeConfig", out object hasKubeConfig) && (bool)hasKubeConfig)
                throw new InvalidOperationException($"{nameof(WithKubeConfig)} can only be called once per execution context.");
            
            return this.WithArgument($"--kubeconfig={configPath}").WithState("hasKubeConfig", true);
        }
    }
}