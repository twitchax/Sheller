using Sheller.Models;

namespace Sheller.Implementations
{
    /// <summary>
    /// The default result type for executables.
    /// </summary>
    public class CommandEvent : ICommandEvent
    {
        /// <summary>
        /// StreamType property.
        /// </summary>
        /// <value>The stream type of the event.</value>
        public CommandEventType Type { get; private set; }

        /// <summary>
        /// Data property.
        /// </summary>
        /// <value>The string data of the event.</value>
        public string Data { get; private set; }

        /// <summary>
        /// The CommandEvent constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public CommandEvent(CommandEventType type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}