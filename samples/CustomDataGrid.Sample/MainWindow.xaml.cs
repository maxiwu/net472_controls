using System.Windows;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Controls;
using CustomDataGrid.Sample.ViewModels;

namespace CustomDataGrid.Sample
{
    /// <summary>
    /// The sample window. Owns a <see cref="MainViewModel"/> and forwards the
    /// grid's six outbound routed events to it for logging (Task 8.6) — the
    /// events are routed events, so they are subscribed here in the view rather
    /// than bound from the view model.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Initializes the window, its view model, and the routed-event hookup.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Grid.SelectedRowChanged += OnSelectedRowChanged;
            Grid.SelectedRowsChanged += OnSelectedRowsChanged;
            Grid.GroupExpanded += OnGroupExpanded;
            Grid.GroupCollapsed += OnGroupCollapsed;
            Grid.CellEditCommitted += OnCellEditCommitted;
            Grid.CellEditCancelled += OnCellEditCancelled;
        }

        private void OnSelectedRowChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.OnSelectedRowChanged((SelectedRowChangedEventArgs)e);
        }

        private void OnSelectedRowsChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.OnSelectedRowsChanged((SelectedRowsChangedEventArgs)e);
        }

        private void OnGroupExpanded(object sender, RoutedEventArgs e)
        {
            _viewModel.OnGroupExpanded((GroupExpandedEventArgs)e);
        }

        private void OnGroupCollapsed(object sender, RoutedEventArgs e)
        {
            _viewModel.OnGroupCollapsed((GroupCollapsedEventArgs)e);
        }

        private void OnCellEditCommitted(object sender, RoutedEventArgs e)
        {
            _viewModel.OnCellEditCommitted((CellEditCommittedEventArgs)e);
        }

        private void OnCellEditCancelled(object sender, RoutedEventArgs e)
        {
            _viewModel.OnCellEditCancelled((CellEditCancelledEventArgs)e);
        }
    }
}
