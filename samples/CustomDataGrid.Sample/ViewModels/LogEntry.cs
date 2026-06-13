using System;

namespace CustomDataGrid.Sample.ViewModels
{
    /// <summary>
    /// A single immutable entry in the event log (Task 8.5): when it happened,
    /// its <see cref="LogCategory"/>, and the human-readable message.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new log entry stamped with the current time.
        /// </summary>
        /// <param name="category">The entry category.</param>
        /// <param name="message">The message text.</param>
        public LogEntry(LogCategory category, string message)
        {
            Timestamp = DateTime.Now;
            Category = category;
            Message = message;
        }

        /// <summary>Gets the time the entry was created.</summary>
        public DateTime Timestamp { get; }

        /// <summary>Gets the entry category.</summary>
        public LogCategory Category { get; }

        /// <summary>Gets the message text.</summary>
        public string Message { get; }
    }
}
