using System.Linq;
using System.Windows;
using System.Windows.Threading;
using INCOMSYSTEM.Context;

namespace INCOMSYSTEM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var authUser = INCOMSYSTEM.Windows.MainWindow.AuthUser;
            if (authUser == null) return;
            using (var db = new INCOMSYSTEMEntities())
            {
                var user = db.UsersDetail.First(s => s.idUser == authUser.idUser);
                user.isOnline = false;
                db.SaveChanges();
            }
            
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var authUser = INCOMSYSTEM.Windows.MainWindow.AuthUser;
            if (authUser == null) return;
            using (var db = new INCOMSYSTEMEntities())
            {
                var user = db.UsersDetail.First(s => s.idUser == authUser.idUser);
                user.isOnline = false;
                db.SaveChanges();
            }
        }
    }
}