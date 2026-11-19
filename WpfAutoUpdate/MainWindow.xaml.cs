using System.IO;
using System.Reflection;
using System.Windows;
using AutoUpdaterDotNET;

namespace WpfAutoUpdate;

public partial class MainWindow : Window
{
   
    private static readonly string DefaultUpdateUrl =
        new Uri(Path.Combine(AppContext.BaseDirectory, "update.xml")).AbsoluteUri;

    public MainWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = "1.9.3.0";
        UrlBox.Text = DefaultUpdateUrl;

        
        AutoUpdater.ShowSkipButton = true;
        AutoUpdater.ShowRemindLaterButton = true;
        AutoUpdater.RunUpdateAsAdmin = false;
        AutoUpdater.Synchronous = false;

        Loaded += (_, _) => AutoUpdater.Start(UrlBox.Text);
    }

    private void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        AutoUpdater.CheckForUpdateEvent -= OnCheckForUpdate;
        if (SilentCheck.IsChecked == true)
            AutoUpdater.CheckForUpdateEvent += OnCheckForUpdate;

        AutoUpdater.ReportErrors = true;   
        Log($"Проверка: {UrlBox.Text}");
        AutoUpdater.Start(UrlBox.Text);
    }

   
    private void OnCheckForUpdate(UpdateInfoEventArgs args)
    {
        Dispatcher.Invoke(() =>
        {
            if (args.Error != null)
            {
                Log($"Ошибка: {args.Error.Message}");
                MessageBox.Show(args.Error.Message, "Ошибка проверки обновлений",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!args.IsUpdateAvailable)
            {
                Log("Обновлений нет.");
                MessageBox.Show("У вас последняя версия.", "Обновление");
                return;
            }

            Log($"Доступна версия {args.CurrentVersion} (установлена {args.InstalledVersion}).");
            var answer = MessageBox.Show(
                $"Доступна новая версия {args.CurrentVersion}.\nСкачать и установить?",
                "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes) return;

            try
            {
                if (AutoUpdater.DownloadUpdate(args))
                    Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Log($"Ошибка загрузки: {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    private void Log(string text) =>
        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
}
