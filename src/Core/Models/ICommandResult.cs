
namespace Sheller.Models
{
    public interface ICommandResult
    {
        int ExitCode { get; }
        string StandardOutput { get; }
        string ErrorOutput { get; }
    }
}