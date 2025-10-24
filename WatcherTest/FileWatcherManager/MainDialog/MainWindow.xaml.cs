using System.Windows;

using Microsoft.Win32;

namespace MainDialog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel viewModel;
        public ViewModel @ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;
                this.DataContext = viewModel;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel = new ViewModel();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel?.Watcher?.Dispose();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel?.Dispose();
            this.Close();
        }

        private void SelectDir_Click(object sender, RoutedEventArgs e)
        {

            var startDir = viewModel?.Watcher != null
                ? viewModel.WatchedPath
                : Environment.CurrentDirectory;

            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = startDir,
                Filter = "All Files (*.*)|*.*",
                Title = "Select a file",
                CheckPathExists = true,
            };

            if (openFileDialog.ShowDialog() ?? false)
            {
                viewModel?.WatchPath(openFileDialog.FileName);
            }
        }

    }
}