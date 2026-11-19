using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WpfApi;

public partial class MainWindow : Window
{
    private readonly ApiClient _api = new();
    private readonly ObservableCollection<Post> _posts = [];

    public MainWindow()
    {
        InitializeComponent();
        PostsGrid.ItemsSource = _posts;
        Closed += (_, _) => _api.Dispose();
    }

    private async void LoadUsers_Click(object sender, RoutedEventArgs e) =>
        await RunAsync("Загрузка пользователей...", async () =>
        {
            var users = await _api.GetUsersAsync();
            UsersList.ItemsSource = users;
            return $"Загружено пользователей: {users.Count}";
        });

    private async void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UsersList.SelectedItem is not User user) return;

        await RunAsync($"Загрузка постов {user.Name}...", async () =>
        {
            var posts = await _api.GetPostsAsync(user.Id);
            _posts.Clear();
            foreach (var p in posts) _posts.Add(p);
            PostsHeader.Text = $"Посты: {user.Name}";
            return $"Загружено постов: {posts.Count}";
        });
    }

    private async void AddPost_Click(object sender, RoutedEventArgs e)
    {
        if (UsersList.SelectedItem is not User user)
        {
            MessageBox.Show("Сначала выберите пользователя.");
            return;
        }
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            MessageBox.Show("Введите заголовок.");
            return;
        }

        await RunAsync("Отправка...", async () =>
        {
            var created = await _api.CreatePostAsync(user.Id, TitleBox.Text, BodyBox.Text);
            if (created != null) _posts.Insert(0, created);
            TitleBox.Clear();
            BodyBox.Clear();
            return $"Создан пост Id={created?.Id} (тестовый API не сохраняет данные)";
        });
    }

    private async void DeletePost_Click(object sender, RoutedEventArgs e)
    {
        if (PostsGrid.SelectedItem is not Post post)
        {
            MessageBox.Show("Выберите пост в таблице.");
            return;
        }

        await RunAsync("Удаление...", async () =>
        {
            await _api.DeletePostAsync(post.Id);
            _posts.Remove(post);
            return $"Пост Id={post.Id} удалён";
        });
    }

   
    private async Task RunAsync(string status, Func<Task<string>> action)
    {
        StatusText.Text = status;
        IsEnabled = false;
        try
        {
            StatusText.Text = await action();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Ошибка: " + ex.Message;
            MessageBox.Show(ex.Message, "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsEnabled = true;
        }
    }
}
