using System.Windows;
using Borne.Services;
using Borne.Views;

namespace Borne
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Charge la configuration de la borne (nom du resto, couleurs, etc.)
            BorneConfig.Instance.Load();

            // Fenêtre de login
            var login = new LoginWindow();
            bool? ok = login.ShowDialog();

            if (ok == true)
            {
                var main = new MainWindow();
                main.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
