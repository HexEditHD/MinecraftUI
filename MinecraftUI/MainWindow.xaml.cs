using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MinecraftUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Process ServerProc;

        public MainWindow()
        {
            InitializeComponent();

            ObjectQuery objectQuery = new("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher managementObjectSearcher = new(objectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                Int64 totalRamMB = Convert.ToInt64(managementObject["TotalVisibleMemorySize"]) / 1024;

                Starter_MinRamSlider.Minimum = 0;
                Starter_MaxRamSlider.Minimum = 0;
                Starter_MinRamSlider.Maximum = totalRamMB;
                Starter_MaxRamSlider.Maximum = totalRamMB;
            }

            if (Starter_MinRamText.Text == null)
            {
                Starter_MinRamText.Text = "0";
            }
            if (Starter_MaxRamText.Text == null)
            {
                Starter_MaxRamText.Text = "0";
            }


            // If the values are already there then just load them.
            string ServerFile = "server.jar";
            string ServerPath = "C:\\Users\\Fayez\\OneDrive\\Desktop\\asd";


            var startInfo = new ProcessStartInfo("java", "-Xmx" + Starter_MinRamText.Text + "M -Xms1024M -jar " + ServerFile + " nogui")
            {
                WorkingDirectory = ServerPath
            };

            Console_ConsoleOutputText.Text += "java -Xmx" + Starter_MinRamText.Text + "M -Xms" + Starter_MinRamText.Text + "M -jar " + ServerFile + " nogui";
            startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false; // Necessary for Standard Stream Redirection
            startInfo.CreateNoWindow = true; // You can do either this or open it with "javaw" instead of "java"

            ServerProc = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            ServerProc.OutputDataReceived += new DataReceivedEventHandler(ServerProc_ErrorDataReceived);
            ServerProc.Exited += new EventHandler(ServerProc_Exited);
        }

        private void Starter_BrowseJarFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
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

        private void ServerProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // You have to do this through the Dispatcher because this method is called by a different Thread
            Dispatcher.Invoke(new Action(() =>
            {
                Console_ConsoleOutputText.Text += e.Data + "\r\n";
                Console_ConsoleInputText.ScrollToEnd();
            }));
        }

        private void ServerProc_Exited(object sender, EventArgs e)
        {
            // The order of these 2 lines is very important, reversing them will cause an exception
            // and you wont be able to read from the stream when you start the Process again !
            ServerProc.CancelOutputRead();
            ServerProc.Close();
        }

        private void Console_ConsoleInputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ServerProc.StandardInput.WriteLine(Console_ConsoleInputText.Text);
            Console_ConsoleInputText.Clear();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                ServerProc.StandardInput.WriteLine("stop");
                ServerProc.WaitForExit(10000);
                if (!ServerProc.HasExited)
                {
                    Console_ConsoleOutputText.Text += "ERROR: The Server doesn't want to Stop !\r\n";
                    e.Cancel = true;
                }
            }
            catch { }
        }

        private void Console_StartBtn_Click(object sender, RoutedEventArgs e)
        {
            // This is my dirty way of making sure that I don't start the Server Twice :D
            // If the server isn't running then an exception will be thrown when accessing
            // ServerProc.StartTime and the method wont return ;-)
            try { var x = ServerProc.StartTime; return; }
            catch { }

            Console_ConsoleOutputText.Text = "";
            ServerProc.Start();
            ServerProc.BeginOutputReadLine();
        }

        private void Console_StopBtn_Click(object sender, RoutedEventArgs e)
        {
            try { ServerProc.StandardInput.WriteLine("stop");}
            catch { }
        }

        private void Console_RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                ServerProc.StandardInput.WriteLine("stop");
                var x = ServerProc.StartTime; return;
            }
            catch { }

            Console_ConsoleOutputText.Text = "";
            ServerProc.Start();
            ServerProc.BeginOutputReadLine();

        }
        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
