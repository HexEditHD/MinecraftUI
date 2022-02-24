using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Management;
using System.ComponentModel;

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

            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(objectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                Int64 totalRamMB = Convert.ToInt64(managementObject["TotalVisibleMemorySize"])/1024;
                
                Starter_MinRamSlider.Minimum = 0;
                Starter_MaxRamSlider.Minimum = 0;
                Starter_MinRamSlider.Maximum = totalRamMB;
                Starter_MaxRamSlider.Maximum = totalRamMB;
            }
            

        }

        private void Starter_BrowseJarFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            
            {
                Title = "Select server file",
                Filter = "Java .jar files |*.jar",
                CheckFileExists = true,
                Multiselect = false
            };
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.ShowDialog();
            Starter_JarFileText.Text = dialog.FileName;
        }

        /// <summary>
        /// Sets icon, labels and icons to appropriate server based on selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Starter_ServerTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = ((ComboBoxItem)Starter_ServerTypeCombo.SelectedItem).Content.ToString();
            Image Starter_ServerTypeImage = new Image();
            BitmapImage src = new();

            
            if (selectedValue == "Spigot")
            {



                src.BeginInit();
                src.UriSource = new Uri("/assets/spigot_logo.png", UriKind.Relative);
                src.EndInit();
                Starter_ServerTypeImage.Source = src;
            }
            if (selectedValue == "Vanilla")
            {
                src.BeginInit();
                src.UriSource = new Uri("/assets/vanilla_logo.png", UriKind.Relative);
                src.EndInit();
                Starter_ServerTypeImage.Source = src;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessStartInfo psi = new()
            {
                FileName = "java.exe",
                Arguments = " -version",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            Process pr = Process.Start(psi);
            string strOutput = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");

            Starter_JarFileText.Text = strOutput;
            //if(strOutput == )
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileAsync(new System.Uri("https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar"),
                 "BuildTools.jar");

            }
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            else
                MessageBox.Show("Completed!!!");
        }

    }
}
