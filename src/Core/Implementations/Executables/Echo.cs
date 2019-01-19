

namespace Sheller.Implementations.Executables
{
    public class Echo : Executable<string>
    {
        public Echo() : base("echo", null, cr => cr.StandardOutput.Trim())
        {

        }
    }
}