

using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    public class Sleep : Executable<Sleep, string>
    {
        public override Task<string> ExecuteAsync() => this.ExecuteAsync(cr => cr.StandardOutput.Trim());

        public override Sleep Initialize(IShell shell)
        {
            return this.Initialize("sleep", shell);
        }
    }
}