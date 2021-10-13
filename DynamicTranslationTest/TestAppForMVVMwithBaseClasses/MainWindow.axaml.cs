using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading;
using TestAppForMVVMwithBaseClasses.Localization;

namespace TestAppForMVVMwithBaseClasses
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var curCult = Thread.CurrentThread.CurrentUICulture;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            TranslatedText();
        }

        private void OnDutchClick(object sender, RoutedEventArgs e)
        {
            SetLanguage("nl-NL");
            TranslatedText();
        }

        private void OnEnglishClick(object sender, RoutedEventArgs e)
        {
            SetLanguage("en");
            TranslatedText();
        }

        public void TranslatedText()
        {
            this.FindControl<TextBlock>("TranslatedText1").Text = TranslationSourceWithMultipleResxFiles.Instance["TestAppForMVVMwithBaseClasses.Properties.Resources.String1"]; 
        }

        public static void SetLanguage(string locale)
        {
            if (string.IsNullOrEmpty(locale)) locale = "en";
            TranslationSourceWithMultipleResxFiles.Instance.CurrentCulture = new System.Globalization.CultureInfo(locale);
        }
    }
}