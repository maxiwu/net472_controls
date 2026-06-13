using CustomDataGrid.Models;

namespace CustomDataGrid.Sample.Models
{
    /// <summary>
    /// A sample group row that extends <see cref="GridGroupRow"/> with an
    /// editable <see cref="Description"/> (text) and a <see cref="Status"/>
    /// (Enable / Disable) shown in a combo box column.
    /// </summary>
    public class SampleGroupRow : GridGroupRow
    {
        private string _description;
        private string _status;

        /// <summary>
        /// Gets or sets the editable group description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the group status: <c>"Enable"</c> or <c>"Disable"</c>.
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged();
            }
        }
    }
}
