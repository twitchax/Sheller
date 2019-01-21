

using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    public class Echo : Executable<Echo, string>
    {
        public override Task<string> ExecuteAsync()
        {
            return this.ExecuteAsync(cr => cr.StandardOutput.Trim());
        }

        public override Echo Initialize(IShell shell)
        {
            return this.Initialize("echo", shell);
        }
    }
}