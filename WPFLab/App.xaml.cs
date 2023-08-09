using PresentationTheme.Aero;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WPFGedcomParser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            // Set theme resources
            AeroTheme.SetAsCurrentTheme();

            // Shortcut for:
            // ThemeManager.SetPresentationFrameworkTheme(new AeroThemePolicy());

        }
    }
}
