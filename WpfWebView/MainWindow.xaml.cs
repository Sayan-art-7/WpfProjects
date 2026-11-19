using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace WpfWebView;

public partial class MainWindow : Window
{
    private const string DemoHtml = """
        <!doctype html>
        <html><head><meta charset="utf-8"><title>Demo</title>
        <style>
          body{font-family:Segoe UI;padding:32px;background:#1E1E2E;color:#E6E6F0}
          input{background:#2A2A3C;color:#E6E6F0;border:1px solid #45455E;border-radius:8px;padding:8px}
          button{background:#8B5CF6;color:#fff;border:0;border-radius:8px;padding:8px 16px;cursor:pointer}
          b{color:#A07BFA}
        </style></head>
        <body>
          <h2>Обмен сообщениями WebView2</h2>
          <input id="msg" value="Привет из JavaScript!" style="width:300px;padding:4px">
          <button onclick="window.chrome.webview.postMessage(document.getElementById('msg').value)">
            Отправить в C#
          </button>
          <p>Сообщение из C#: <b id="fromCs">—</b></p>
          <script>
            window.chrome.webview.addEventListener('message', e =>
              document.getElementById('fromCs').textContent = e.data);
          </script>
        </body></html>
        """;

    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
       
            await WebView.EnsureCoreWebView2Async();

            WebView.CoreWebView2.NavigationStarting += (_, e) => StatusText.Text = "Загрузка: " + e.Uri;
            WebView.CoreWebView2.NavigationCompleted += (_, e) =>
                StatusText.Text = e.IsSuccess ? "Готово" : $"Ошибка: {e.WebErrorStatus}";
            WebView.CoreWebView2.SourceChanged += (_, _) => AddressBox.Text = WebView.Source?.ToString();
            WebView.CoreWebView2.DocumentTitleChanged += (_, _) =>
                Title = $"{WebView.CoreWebView2.DocumentTitle} — WebView2";

          
            WebView.CoreWebView2.WebMessageReceived += (_, e) =>
                MessageBox.Show(e.TryGetWebMessageAsString(), "Сообщение из JS");

            WebView.CoreWebView2.NewWindowRequested += (_, e) =>
            {
                e.Handled = true;
                WebView.CoreWebView2.Navigate(e.Uri);
            };
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show("Не установлен Microsoft Edge WebView2 Runtime.\n" +
                            "Скачайте: https://developer.microsoft.com/microsoft-edge/webview2/",
                "WebView2", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Navigate()
    {
        var url = AddressBox.Text.Trim();
        if (string.IsNullOrEmpty(url)) return;

        if (!url.Contains("://"))
            url = url.Contains('.') && !url.Contains(' ')
                ? "https://" + url
                : "https://www.bing.com/search?q=" + Uri.EscapeDataString(url);

        if (WebView.CoreWebView2 != null)
            WebView.CoreWebView2.Navigate(url);
    }

    private void Go_Click(object sender, RoutedEventArgs e) => Navigate();

    private void AddressBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Navigate();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CanGoBack) WebView.GoBack();
    }

    private void Forward_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CanGoForward) WebView.GoForward();
    }

    private void Reload_Click(object sender, RoutedEventArgs e) => WebView.Reload();

    private async void SendToJs_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CoreWebView2 == null) return;

     
        void OnLoaded(object? s, CoreWebView2NavigationCompletedEventArgs args)
        {
            WebView.CoreWebView2.NavigationCompleted -= OnLoaded;
            WebView.CoreWebView2.PostWebMessageAsString($"Привет из C#! Время: {DateTime.Now:T}");
        }

        WebView.CoreWebView2.NavigationCompleted += OnLoaded;
        WebView.NavigateToString(DemoHtml);

   
        await Task.Delay(500);
        var userAgent = await WebView.CoreWebView2.ExecuteScriptAsync("navigator.userAgent");
        StatusText.Text = "navigator.userAgent = " + userAgent;
    }
}
