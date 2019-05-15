using System;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The interface for `helm`.
    /// </summary>
    public interface IHelm : IExecutable<IHelm>
    {
        /// <summary>
        /// Adds a kubeconfig argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.  It is not required, but convention
        /// would dictate that this method be called before arguments.
        /// </remarks>
        /// <param name="configPath"></param>
        /// <returns>A `new` instance of <see cref="Helm"/> with the kubeconfig passed to this call.</returns>
        IHelm WithKubeConfig(string configPath);
    }

    /// <summary>
    /// The executable type for `helm`.
    /// </summary>
    public class Helm : Executable<IHelm>, IHelm
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Helm"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Executable<IHelm> Create() => new Helm();

        /// <summary>
        /// The <cref see="Helm"/> constructor.
        /// </summary>
        public Helm() : base("helm") {}

        /// <summary>
        /// Adds a kubeconfig argument to the execution context and returns a `new` context instance.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method in one execution context will trigger an exception.  It is not required, but convention
        /// would dictate that this method be called before arguments.
        /// </remarks>
        /// <param name="configPath"></param>
        /// <returns>A `new` instance of <see cref="Helm"/> with the kubeconfig passed to this call.</returns>
        public IHelm WithKubeConfig(string configPath)
        {
            if(this.State.TryGetValue("hasKubeConfig", out object hasKubeConfig) && (bool)hasKubeConfig)
                throw new InvalidOperationException($"{nameof(WithKubeConfig)} can only be called once per execution context.");
            
            return this.WithState("hasKubeConfig", true).WithArgument($"--kubeconfig={configPath}");
        }
    }
}