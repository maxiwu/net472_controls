using System.Collections.ObjectModel;

namespace CustomDataGrid.Sample.ViewModels
{
    /// <summary>
    /// Backs the live event log panel (Task 8.5). Entries are kept newest-first
    /// (inserted at index 0) so the most recent event is always at the top.
    /// </summary>
    public class LogViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the log entries, newest first.
        /// </summary>
        public ObservableCollection<LogEntry> Entries { get; } = new ObservableCollection<LogEntry>();

        /// <summary>
        /// Gets the number of entries (for the "{n} events" label).
        /// </summary>
        public int EntryCount
        {
            get { return Entries.Count; }
        }

        /// <summary>
        /// Adds a new entry at the top of the log.
        /// </summary>
        /// <param name="category">The entry category.</param>
        /// <param name="message">The message text.</param>
        public void AddEntry(LogCategory category, string message)
        {
            Entries.Insert(0, new LogEntry(category, message));
            OnPropertyChanged(nameof(EntryCount));
        }

        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void Clear()
        {
            Entries.Clear();
            OnPropertyChanged(nameof(EntryCount));
        }
    }
}
