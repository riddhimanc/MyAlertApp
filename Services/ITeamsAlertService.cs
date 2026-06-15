using ReportApp.Services;

namespace ReportApp.Services;

public interface ITeamsAlertService
{
    Task SendAlertToChatAsync(List<UrlStatus> urlStatuses);
    Task SendAlertToChannelAsync(List<UrlStatus> urlStatuses);
}
