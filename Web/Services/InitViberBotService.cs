using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Viber.Bot.NetCore.Infrastructure;
using Viber.Bot.NetCore.Models;
using Viber.Bot.NetCore.RestApi;
using Web.Configurations;

namespace Web.Services
{
    public class InitViberBotService : IHostedService
    {
        private readonly IViberBotApi _viberBotApi;
        private readonly IOptionsMonitor<BotConfiguration> _botConfiguration;

        public InitViberBotService(IViberBotApi viberBotApi, IOptionsMonitor<BotConfiguration> botConifguration)
        {
            _viberBotApi = viberBotApi;
            _botConfiguration = botConifguration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Refit.ApiResponse<ViberWebHook.ViberWebHookResponse> response = await _viberBotApi.SetWebHookAsync(new ViberWebHook.WebHookRequest("https://591f-176-119-235-147.ngrok-free.app/bot"));
            return _viberBotApi.SetWebHookAsync(new ViberWebHook.WebHookRequest()
            {
                Url = $"{_botConfiguration.CurrentValue.Webhook}/bot",
                EventTypes = new() { ViberEventType.Message, ViberEventType.Subscribed },
                SendName = false,
                SendPhoto = false
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _viberBotApi.SetWebHookAsync(new ViberWebHook.WebHookRequest(""));
        }
    }
}