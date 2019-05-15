using System;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The interface for `kubectl`.
    /// </summary>
    public interface IKubectl : IExecutable<IKubectl>
    {
        /// <summary>
        /// Adds a kubeconfig argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.  It is not required, but convention
        /// would dictate that this method be called before arguments.
        /// </remarks>
        /// <param name="configPath">The path to the config file.</param>
        /// <returns>A `new` instance of <see cref="Kubectl"/> with the kubeconfig passed to this call.</returns>
        IKubectl WithKubeConfig(string configPath);

        /// <summary>
        /// Adds an apply argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.
        /// </remarks>
        /// <param name="yamlPath">The path to the YAML file.</param>
        /// <returns>A `new` instance of <see cref="Kubectl"/> with the apply YAML passed to this call.</returns>
        IKubectl WithApply(string yamlPath);
    }

    /// <summary>
    /// The executable type for `kubectl`.
    /// </summary>
    public class Kubectl : Executable<IKubectl>, IKubectl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Kubectl"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Executable<IKubectl> Create() => new Kubectl();

        /// <summary>
        /// The <cref see="Kubectl"/> constructor.
        /// </summary>
        public Kubectl() : base("kubectl") {}

        /// <summary>
        /// Adds a kubeconfig argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.  It is not required, but convention
        /// would dictate that this method be called before arguments.
        /// </remarks>
        /// <param name="configPath">The path to the config file.</param>
        /// <returns>A `new` instance of <see cref="Kubectl"/> with the kubeconfig passed to this call.</returns>
        public IKubectl WithKubeConfig(string configPath)
        {
            if(this.State.TryGetValue("hasKubeConfig", out object hasKubeConfig) && (bool)hasKubeConfig)
                throw new InvalidOperationException($"{nameof(WithKubeConfig)} can only be called once per execution context.");
            
            return this.WithState("hasKubeConfig", true).WithArgument($"--kubeconfig={configPath}") as IKubectl;
        }

        /// <summary>
        /// Adds an apply argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.
        /// </remarks>
        /// <param name="yamlPath">The path to the YAML file.</param>
        /// <returns>A `new` instance of <see cref="Kubectl"/> with the apply YAML passed to this call.</returns>
        public IKubectl WithApply(string yamlPath)
        {
            if(this.State.TryGetValue("hasApply", out object hasApply) && (bool)hasApply)
                throw new InvalidOperationException($"{nameof(WithApply)} can only be called once per execution context.");
            
            return this.WithState("hasApply", true).WithArgument("apply", "-f", yamlPath) as IKubectl;
        }
    }
}