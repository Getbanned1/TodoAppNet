using System.Windows;

namespace TodoAppNet
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

                  //var authView = new AuthView();
                  //authView.Show();

            //    //// Закрытие приложения при закрытии окна авторизации
            //    //authView.Closed += (s, args) => Current.Shutdown();
        }
    }
}