
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    public class Generic : Executable<ICommandResult>
    {
        public Generic(string executable, IShell shell) : base(executable, shell, cr => cr)
        {
            // TODO: Use Impl methods to define the defaults.
        }
    }
}