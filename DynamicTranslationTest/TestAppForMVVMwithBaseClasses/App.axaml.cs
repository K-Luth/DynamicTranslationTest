using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TestAppForMVVMwithBaseClasses.ViewModels;

namespace TestAppForMVVMwithBaseClasses
{
    public class App : Application
    {
        public override void Initialize()
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("nl-NL");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en"); // sets the UI translations to CultureInfo(CultureName) on startup.

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
