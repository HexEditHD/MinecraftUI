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
                Int64 totalRamMB = Convert.ToInt64(managementObject["TotalVisibleMemorySize"]) / 1024;

                
                Starter_MinRamSlider.Maximum = totalRamMB;
                Starter_MaxRamSlider.Maximum = totalRamMB;
            }

            if (Starter_MinRamSlider.Minimum < 0)
            {
                Starter_MinRamSlider.Minimum = 0;
            }
            if (Starter_MaxRamSlider.Minimum < 0)
            {
                Starter_MaxRamSlider.Minimum = 0;
            }
            
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey subKey = rk.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment");

            string currentVerion = subKey.GetValue("CurrentVersion").ToString();

            Starter_JavaComboBox.Items.Add(currentVerion);

            //try
            //{
            //    ProcessStartInfo psi = new ProcessStartInfo();
            //    psi.FileName = "java.exe";
            //    psi.Arguments = " -version";
            //    psi.RedirectStandardError = true;
            //    psi.UseShellExecute = false;

            //    Process pr = Process.Start(psi);
            //    string strOutput = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");

            //    Starter_JavaComboBox.Items.Add(strOutput);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Exception is " + ex.Message);
            //}
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
                src.UriSource = new Uri("../../assets/spigot_logo.png", UriKind.RelativeOrAbsolute);
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

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using WebClient wc = new();

            Starter_Download_Content_Label.Content = "Downloading...";
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.Headers.Add("User-Agent: Other");
            wc.DownloadFile("https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar", "BuildTools.jar");
            Starter_Download_Content_Label.Content = "Downloaded";
        }
    }
}