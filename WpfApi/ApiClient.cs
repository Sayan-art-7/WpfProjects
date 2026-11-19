using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WpfApi;

public record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string Phone);

public record Post(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body);

/// <summary>
/// Клиент REST API. Для примера используется открытый тестовый API JSONPlaceholder.
/// Замените BaseAddress и модели на своё API.
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://jsonplaceholder.typicode.com/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<List<User>> GetUsersAsync() =>
        await _http.GetFromJsonAsync<List<User>>("users") ?? [];

    public async Task<List<Post>> GetPostsAsync(int userId) =>
        await _http.GetFromJsonAsync<List<Post>>($"posts?userId={userId}") ?? [];

    public async Task<Post?> CreatePostAsync(int userId, string title, string body)
    {
        var response = await _http.PostAsJsonAsync("posts", new { userId, title, body });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Post>();
    }

    public async Task DeletePostAsync(int id)
    {
        var response = await _http.DeleteAsync($"posts/{id}");
        response.EnsureSuccessStatusCode();
    }

    public void Dispose() => _http.Dispose();
}
