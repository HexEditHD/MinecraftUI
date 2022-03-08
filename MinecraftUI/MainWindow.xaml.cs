using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MinecraftUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ProcessStartInfo startInfo = new ();
        Process ServerProc;
        public MainWindow()
        {
            InitializeComponent();
            LoadConsole();
            QueryRam();
            
        }

        private void QueryRam()
        {
            ObjectQuery objectQuery = new("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher managementObjectSearcher = new(objectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                Int64 totalRamMB = Convert.ToInt64(managementObject["TotalVisibleMemorySize"]) / 1024;

                Starter_MinRamText.Text = "1024";
                Starter_MaxRamText.Text = "2048";
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
        }

        private void LoadConsole()
        {
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            ServerProc = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            ServerProc.OutputDataReceived += new DataReceivedEventHandler(ServerProc_OutputDataReceived);
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

        private void ServerProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Console_ConsoleOutputText.Focus();
                Console_ConsoleOutputText.AppendText(e.Data + "\r");
                Console_ConsoleInputText.ScrollToEnd();
            }));
        }

        private void ServerProc_Exited(object sender, EventArgs e)
        {
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
                    Console_ConsoleOutputText.AppendText("ERROR: The Server doesn't want to Stop !");
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

           
            if (Starter_ServerTypeCombo.Text == "Minecraft Server")
            {
                startInfo.FileName = "java";
                startInfo.Arguments = "-Xmx4096M -Xms1024M -jar " + Starter_JarFileText.Text + " nogui";
                Console_ConsoleOutputText.SelectAll();
                Console_ConsoleOutputText.Selection.Text = "";
                ServerProc.Start();
                ServerProc.BeginOutputReadLine();

            }
            if (Starter_ServerTypeCombo.Text == "BuildTools")
            {
                Process buildToolsProcess = new();
                buildToolsProcess.StartInfo.FileName = "cmd";
                buildToolsProcess.StartInfo.Arguments = "/C java -jar " + Starter_JarFileText.Text + " --rev latest";
                buildToolsProcess.StartInfo.CreateNoWindow = false;
                buildToolsProcess.Start();
                buildToolsProcess.WaitForExitAsync();
            }
        }

        private void Console_StopBtn_Click(object sender, RoutedEventArgs e)
        {
            try { ServerProc.StandardInput.WriteLine("stop"); }
            catch { }
        }

        private void Console_RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerProc.StandardInput.WriteLine("stop");
                var x = ServerProc.StartTime; return;
            }
            catch { }

            Console_ConsoleOutputText.SelectAll();
            Console_ConsoleOutputText.Selection.Text = "";
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
