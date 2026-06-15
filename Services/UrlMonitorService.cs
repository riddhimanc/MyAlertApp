using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ReportApp.Models;

namespace ReportApp.Services;

public class UrlMonitorService : IUrlMonitorService
{
    private readonly List<AppUrl> _appUrls;
    private readonly IConfiguration _configuration;

    public UrlMonitorService(IConfiguration configuration)
    {
        _configuration = configuration;
        _appUrls = _configuration.GetSection("AppUrls").Get<List<AppUrl>>() ?? new List<AppUrl>();
    }

    public async Task<List<UrlStatus>> CheckUrlsAsync()
    {
        var statuses = new List<UrlStatus>();
        var certificatePath = _configuration.GetValue<string>("Certificate:Path");
        var certificatePassword = _configuration.GetValue<string>("Certificate:Password");

        X509Certificate2? certificate = null;
        if (!string.IsNullOrEmpty(certificatePath) && File.Exists(certificatePath))
        {
            certificate = new X509Certificate2(certificatePath, certificatePassword);
        }

        var handler = new HttpClientHandler();
        if (certificate != null)
        {
            handler.ClientCertificates.Add(certificate);
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }

        using var client = new HttpClient(handler);
        foreach (var appUrl in _appUrls)
        {
            var status = new UrlStatus { Url = appUrl.Url };
            try
            {
                var response = await client.GetAsync(appUrl.Url);
                status.IsUp = response.IsSuccessStatusCode;
                status.StatusCode = (int)response.StatusCode;
            }
            catch (Exception ex)
            {
                status.IsUp = false;
                status.ErrorMessage = ex.Message;
            }
            statuses.Add(status);
        }

        return statuses;
    }
}
