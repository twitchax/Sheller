using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The interface for a generic executable.
    /// </summary>
    public interface IGenericExe : IExecutable {}

    /// <summary>
    /// The executable type for a generic executable.
    /// </summary>
    public class GenericExe : Executable<IGenericExe>, IGenericExe
    {
        /// <summary>
        /// Creates a new instance of the <see cref="GenericExe"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Executable<IGenericExe> Create() => new GenericExe(this.Path);

        /// <summary>
        /// The <cref see="GenericExe"/> constructor.
        /// </summary>
        public GenericExe(string executable) : base(executable) {}
    }
}