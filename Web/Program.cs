using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Configurations;
using Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Viber.Bot.NetCore.Middleware;
using Web.Services.Interfaces;
using Web.Services;
using Viber.Bot.NetCore.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("MSSQL"));
builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("ViberBot"));

builder.Services.AddDomainServices((sp, options) =>
{
    IOptionsMonitor<AppConfiguration> configuration = sp.GetRequiredService<IOptionsMonitor<AppConfiguration>>();
    ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    _ = options.UseSqlServer(configuration.CurrentValue.ConnectionString)
        .UseLoggerFactory(loggerFactory);
});

builder.Services.AddHttpClient("viberclient")
    .AddTypedClient((client, sp) =>
    {
        IOptionsMonitor<BotConfiguration> configuration = sp.GetRequiredService<IOptionsMonitor<BotConfiguration>>();
        ViberBotConfiguration botConf = new()
        {
            Token = configuration.CurrentValue.Token,
            Webhook = $"{configuration.CurrentValue.Webhook}"
        };

        return ViberClient.RegisterViberApi(botConf);
    });

builder.Services.AddTransient<IViberService, ViberService>();
builder.Services.AddHostedService<InitViberBotService>();

builder.Services.AddSingleton<IUsersInfo, UsersInfo>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
