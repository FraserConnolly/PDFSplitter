using Microsoft.Win32;
using System;
using System.Windows;
using Path = System.IO.Path;
using File = System.IO.File;

namespace PDFSplitterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SplitterVM vm = new SplitterVM();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
            vm.StartText = Properties.Settings.Default.StartText;
            vm.EndText = Properties.Settings.Default.EndText;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                if ( File.Exists ( openFileDialog.FileName))
                {
                    var documentName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    var dir = Path.GetDirectoryName(openFileDialog.FileName);
                    vm.InputPath = openFileDialog.FileName;
                    vm.OutputPath = Path.Combine(dir, documentName);
                }
            }

            vm.UpdateAllUI();
        }

        private void OutputBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Output folder";
                dialog.ShowNewFolderButton = true;
                
                if (vm.OutputPath != String.Empty && System.IO.Directory.Exists(vm.OutputPath))
                {
                    dialog.SelectedPath = vm.OutputPath;
                }

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if ( result == System.Windows.Forms.DialogResult.OK)
                {
                    vm.OutputPath = dialog.SelectedPath;
                    vm.UpdateAllUI();
                }
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            vm.Start();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartText = vm.StartText;
            Properties.Settings.Default.EndText = vm.EndText;
            Properties.Settings.Default.Save();
            MessageBox.Show("Saved");
        }

        
    }
}
