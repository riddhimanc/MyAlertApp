using ReportApp.Models;

namespace ReportApp.Services;

public interface IUrlMonitorService
{
    Task<List<UrlStatus>> CheckUrlsAsync();
}

public class UrlStatus
{
    public string Url { get; set; } = string.Empty;
    public bool IsUp { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}
