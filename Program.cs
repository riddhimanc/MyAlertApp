using ReportApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configuration files
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("app_urls.json", optional: false, reloadOnChange: true)
    .AddJsonFile("teams.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUrlMonitorService, UrlMonitorService>();
builder.Services.AddScoped<ITeamsAlertService, TeamsAlertService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/alerts/chat", async (IUrlMonitorService urlMonitor, ITeamsAlertService teamsAlert) =>
{
    var statuses = await urlMonitor.CheckUrlsAsync();
    await teamsAlert.SendAlertToChatAsync(statuses);
    return Results.Ok(new { message = "Alert sent to Teams Chat successfully", statuses });
})
.WithName("SendAlertToChat")
.WithOpenApi();

app.MapGet("/api/alerts/channel", async (IUrlMonitorService urlMonitor, ITeamsAlertService teamsAlert) =>
{
    var statuses = await urlMonitor.CheckUrlsAsync();
    await teamsAlert.SendAlertToChannelAsync(statuses);
    return Results.Ok(new { message = "Alert sent to Teams Channel successfully", statuses });
})
.WithName("SendAlertToChannel")
.WithOpenApi();

app.Run();
