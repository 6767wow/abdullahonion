using Discord;
using Discord.WebSocket;
using System.Text;
using System.Text.Json;

class Program
{
    private static DiscordSocketClient _client = null!;
    private static readonly HttpClient _http = new HttpClient();
    private const string WebhookUrl = "https://abdullahonion.net/bot";

    static async Task Main(string[] args)
    {
        string? token = Environment.GetEnvironmentVariable("MTUxNTgyMTMwNjYxOTIzNjM1Mw.G8oL9D.3Sh-axUJO35iGtZ1vLRI8ScfVO3ZIRpemwXI_8");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("smth didnt work");
            Environment.Exit(1);
        }

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        Console.WriteLine("Bot is running");
        await Task.Delay(Timeout.Infinite);
    }

    private static async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        string username = message.Author.Username;
        string content = message.Content;
        string timestamp = message.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        string channel = message.Channel is SocketGuildChannel guildChannel
            ? $"#{guildChannel.Name} ({guildChannel.Guild.Name})"
            : $"#{message.Channel.Name}";

        string formatted = $"[{username}]\n{content} ({timestamp}) ({channel})";

        Console.WriteLine($"Forwarding message from {username}");

        try
        {
            var payload = new { text = formatted };
            string json = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _http.PostAsync(WebhookUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Webhook returned {(int)response.StatusCode}: {body}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to forward message: {ex.Message}");
        }
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine($"[Discord.Net] {log}");
        return Task.CompletedTask;
    }
}
