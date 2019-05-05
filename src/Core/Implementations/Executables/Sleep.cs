using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The interface for `sleep`.
    /// </summary>
    public interface ISleep : IExecutable {}

    /// <summary>
    /// The executable type for `sleep`.
    /// </summary>
    public class Sleep : Executable<ISleep>, ISleep
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Sleep"/> type.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override Executable<ISleep> Create() => new Sleep();

        /// <summary>
        /// The <cref see="Sleep"/> constructor.
        /// </summary>
        public Sleep() : base("sleep") {}
    }
}