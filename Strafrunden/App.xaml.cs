using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        

      
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.Log.Instance.Info("Application Starting...");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Friedrich May\\Strafrunden\\");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            Directory.SetCurrentDirectory(dataPath);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logging.Log.Instance.Fail(e.ExceptionObject.ToString());
            App.Current.Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

            Logging.Log.Instance.Info("Application Closed!");
            Logging.Log.Instance.SafeTo("lastLog.txt");
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logging.Log.Instance.Fail(e.Exception.ToString());
            e.Handled = true;
            App.Current.Shutdown();
        }
    }
}
