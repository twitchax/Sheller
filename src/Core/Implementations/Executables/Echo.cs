using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The interface for `echo`.
    /// </summary>
    public interface IEcho : IExecutable<IEcho, string> {}

    /// <summary>
    /// The executable type for `echo`.
    /// </summary>
    public class Echo : Executable<IEcho, string>, IEcho
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Echo"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Executable<IEcho> Create() => new Echo();

        /// <summary>
        /// The <cref see="Echo"/> constructor.
        /// </summary>
        public Echo() : base("echo") {}

        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <returns>A task which results in a <see cref="string"/> (i.e., the result of the execution).</returns>
        public override Task<string> ExecuteAsync() => this.ExecuteAsync(cr => cr.StandardOutput.Trim());
    }
}