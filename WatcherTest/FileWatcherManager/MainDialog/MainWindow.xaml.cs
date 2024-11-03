using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainDialog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel viewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            viewModel = new ViewModel();

            this.DataContext = viewModel;
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

            var startDir = viewModel?.IsWatchedPathValid ?? false ? viewModel.WatchedPath : Environment.CurrentDirectory;
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = startDir,
                Filter = "All Files (*.*)|*.*",
                Title = "Select a file",
                CheckPathExists = true,
            };

            if (openFileDialog.ShowDialog() ?? false) viewModel?.WatchPath(openFileDialog.FileName);
        }

    }
}