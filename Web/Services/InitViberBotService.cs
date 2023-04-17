using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Viber.Bot;
using Web.Configurations;

namespace Web.Services
{
    public class InitViberBotService : BackgroundService
    {
        private readonly IViberBotClient _viberBotClient;
        private readonly IOptionsMonitor<BotConfiguration> _botConfiguration;

        public InitViberBotService(IViberBotClient viberBotClient, IOptionsMonitor<BotConfiguration> botConifguration)
        {
            _viberBotClient = viberBotClient;
            _botConfiguration = botConifguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            _ = await _viberBotClient.SetWebhookAsync($"{_botConfiguration.CurrentValue.Webhook}/bot");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return _viberBotClient.SetWebhookAsync("");
        }
    }
}