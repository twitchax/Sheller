using System;

namespace Sheller.Models
{
    /// <summary>
    /// The default result interface for executables.
    /// </summary>
    public interface ICommandEvent
    {
        /// <summary>
        /// StreamType property.
        /// </summary>
        /// <value>The stream type of the event.</value>
        CommandEventType Type { get; }

        /// <summary>
        /// Data property.
        /// </summary>
        /// <value>The string data of the event.</value>
        string Data { get; }
    }

    /// <summary>
    /// The type (stdout, stderr) of the event data.
    /// </summary>
    public enum CommandEventType
    {
        /// <summary>
        /// The Standard Output type.
        /// </summary>
        StandardOutput,
        /// <summary>
        /// The Standard Error type.
        /// </summary>
        StandardError,
        /// <summary>
        /// The Invocation type.
        /// </summary>
        Invocation
    }
}