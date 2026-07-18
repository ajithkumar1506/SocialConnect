using Hangfire;
using Serilog;
using SocialConnect.API.Configuration;
using SocialConnect.API.Infrastructure.ExceptionHandling;
using SocialConnect.API.Services;
using SocialConnect.Application;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Infrastructure;

new EnvironmentConfigurator().LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

var clientSettings = new ClientSettings();
builder.Configuration.GetSection("ClientSettings").Bind(clientSettings);
builder.Services.AddSingleton(clientSettings);

new SwaggerDocumentationConfigurator().Configure(builder.Services);

new JwtAuthenticationConfigurator(builder.Configuration).Configure(builder.Services);

builder.Services.AddApplication();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

await new DatabaseMigrationConfigurator(app).ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapHub<SocialConnect.API.Hubs.PresenceHub>("/hubs/presence");
app.MapHub<SocialConnect.API.Hubs.ChatHub>("/hubs/chat");
app.MapHub<SocialConnect.API.Hubs.NotificationHub>("/hubs/notification");

app.Run();

public partial class Program { }
