using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Data.Entity;
using Mineswooper.DAL;

namespace Mineswooper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //Database.SetInitializer(new ScoreInitializer());
        }
    }
}
