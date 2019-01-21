
namespace Sheller.Models
{
    /// <summary>
    /// The default result interface for executables.
    /// </summary>
    public interface ICommandResult
    {
        /// <summary>
        /// ExitCode property.
        /// </summary>
        /// <value>The exit code of an executable.</value>
        int ExitCode { get; }

        /// <summary>
        /// StandardOutput property.
        /// </summary>
        /// <value>The standard output of an executable.</value>
        string StandardOutput { get; }

        /// <summary>
        /// StandardError property.
        /// </summary>
        /// <value>The standard error of an executable.</value>
        string StandardError { get; }
    }
}