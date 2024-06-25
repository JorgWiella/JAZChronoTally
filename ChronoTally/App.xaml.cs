using ChronoTally.ViewModels;
using ChronoTally.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ChronoTally
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow();
            //mainWindow.DataContext = new MainViewModel(); 
            mainWindow.Show();
        }
    }
}
