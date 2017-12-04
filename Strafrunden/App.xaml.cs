﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        

        private bool OpenFirewall()
        {
            string output = "";
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "advfirewall firewall show rule name=\"Strafrundenserver\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();

            }
            if (!output.Contains("Strafrundenserver"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "advfirewall firewall add rule dir=in action=allow localport=80 protocol=tcp name=\"Strafrundenserver\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "advfirewall firewall add rule dir=out protocol=tcp action=allow localport=80  name=\"Strafrundenserver\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            return true;
        }
        private bool CloseFirewall()
        {
            string output = "";
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "advfirewall firewall show rule name=\"Strafrundenserver\"";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();

            }
            if (output.Contains("Strafrundenserver"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "advfirewall firewall delete rule name=\"Strafrundenserver\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return true;
        }
        private bool AddUrlacl()
        {
            string output = "";

            using (Process p = new Process())
            {
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "http show urlacl";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();

            }
            if (!output.Contains(Strafrunden.Properties.Settings.Default.urlPrefix))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "http add urlacl " + Strafrunden.Properties.Settings.Default.urlPrefix + " user=\"" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "\"";
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Strafrunden.Properties.Settings.Default.ApplicationVersion != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                Strafrunden.Properties.Settings.Default.Upgrade();
                Strafrunden.Properties.Settings.Default.ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }


            if (!OpenFirewall())
            {
                MessageBox.Show("Für die korrekte Funktion des Servers müssen Ports freigegeben werden. Dies benötigt Administratoren-Rechte.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!AddUrlacl())
            {
                MessageBox.Show("Für die korrekte Funktion des Servers müssen URLs freigegeben werden. Dies benötigt Administratoren-Rechte.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            

            
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (!CloseFirewall())
            {
                MessageBox.Show("Für die korrekte Funktion des Servers müssen Ports freigegeben werden. Dies benötigt Administratoren-Rechte.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
