using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The executable type for `kubectl`.
    /// </summary>
    public class Kubectl : Executable<Kubectl>
    {
        /// <summary>
        /// Initializes an <see cref="Kubectl"/> instance with the provided shell.
        /// </summary>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <returns>This instance.</returns>
        public override Kubectl Initialize(IShell shell) => this.Initialize("kubectl", shell);

        /// <summary>
        /// Adds a kubeconfig argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.  It is not required, but convention
        /// would dictate that this method be called before arguments.
        /// </remarks>
        /// <param name="configPath">The path to the config file.</param>
        /// <returns>A `new` instance of <see cref="Kubectl"/> with the kubeconfig passed to this call.</returns>
        public Kubectl WithKubeConfig(string configPath)
        {
            if(this.State.TryGetValue("hasKubeConfig", out object hasKubeConfig) && (bool)hasKubeConfig)
                throw new InvalidOperationException($"{nameof(WithKubeConfig)} can only be called once per execution context.");
            
            return this.WithArgument($"--kubeconfig={configPath}").WithState("hasKubeConfig", true);
        }

        /// <summary>
        /// Adds an apply argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.
        /// </remarks>
        /// <param name="yamlPath">The path to the YAML file.</param>
        /// <returns>A `new` instance of <see cref="Kubectl"/> with the apply YAML passed to this call.</returns>
        public Kubectl WithApply(string yamlPath)
        {
            if(this.State.TryGetValue("hasApply", out object hasApply) && (bool)hasApply)
                throw new InvalidOperationException($"{nameof(WithApply)} can only be called once per execution context.");
            
            return this.WithArgument("apply", "-f", yamlPath).WithState("hasApply", true);
        }
    }
}