using Microsoft.Win32;
using System.Windows;

namespace MinecraftUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Starter_BrowseJarFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select server file",
                Filter = ("Java .jar files |*.jar"),
                CheckFileExists = true,
                Multiselect = false
            };
            dialog.ShowDialog();
            Starter_JarFileText.Text = dialog.FileName;
        }
    }
}
