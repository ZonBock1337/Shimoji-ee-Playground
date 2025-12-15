using ShimojiPlaygroundApp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Shimoji_ee_Playground_Window
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += (s, ex) =>
            {
                Logger.Crash("Application error", ex.Exception);

                MessageBox.Show(
                    "Application error.\nPlease view logs.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                ex.Handled = true;
            };
        }
    }
}
