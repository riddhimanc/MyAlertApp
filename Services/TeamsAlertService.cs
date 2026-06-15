using System.Text;
using System.Text.Json;
using ReportApp.Models;
using ReportApp.Services;

namespace ReportApp.Services;

public class TeamsAlertService : ITeamsAlertService
{
    private readonly TeamsConfig _teamsConfig;
    private readonly HttpClient _httpClient;

    public TeamsAlertService(IConfiguration configuration)
    {
        _teamsConfig = configuration.GetSection("TeamsConfig").Get<TeamsConfig>() ?? new TeamsConfig();
        _httpClient = new HttpClient();
    }

    public async Task SendAlertToChatAsync(List<UrlStatus> urlStatuses)
    {
        await SendAlertAsync(urlStatuses, "URL Monitoring Alert - Chat");
    }

    public async Task SendAlertToChannelAsync(List<UrlStatus> urlStatuses)
    {
        await SendAlertAsync(urlStatuses, "URL Monitoring Alert - Channel");
    }

    private async Task SendAlertAsync(List<UrlStatus> urlStatuses, string title)
    {
        if (string.IsNullOrEmpty(_teamsConfig.WebhookUrl))
        {
            throw new InvalidOperationException("Teams webhook URL is not configured.");
        }

        var adaptiveCard = new Dictionary<string, object>
        {
            ["type"] = "message",
            ["attachments"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["contentType"] = "application/vnd.microsoft.card.adaptive",
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    ["contentUrl"] = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    ["content"] = new Dictionary<string, object>
                    {
                        ["type"] = "AdaptiveCard",
                        ["version"] = "1.2",
                        ["body"] = new object[]
                        {
                            new { type = "TextBlock", size = "Large", weight = "Bolder", text = title },
                            new { type = "TextBlock", text = $"Alert generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}", isSubtle = true },
                            new { type = "FactSet", facts = urlStatuses.Select(u => new { title = u.Url, value = u.IsUp ? $"✅ Up (Status: {u.StatusCode})" : $"❌ Down (Error: {u.ErrorMessage})" }).ToArray() }
                        },
                        ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json"
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(adaptiveCard);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_teamsConfig.WebhookUrl, content);
        response.EnsureSuccessStatusCode();
    }
}
