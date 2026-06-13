using CustomDataGrid.Models;

namespace CustomDataGrid.Sample.Models
{
    /// <summary>
    /// A sample item row that extends <see cref="GridItemRow"/> with two editable
    /// float values, <see cref="X"/> and <see cref="Y"/>.
    /// </summary>
    public class SampleItemRow : GridItemRow
    {
        private float _x;
        private float _y;

        /// <summary>
        /// Gets or sets the editable X value.
        /// </summary>
        public float X
        {
            get { return _x; }
            set
            {
                if (_x == value) return;
                _x = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the editable Y value.
        /// </summary>
        public float Y
        {
            get { return _y; }
            set
            {
                if (_y == value) return;
                _y = value;
                OnPropertyChanged();
            }
        }
    }
}
