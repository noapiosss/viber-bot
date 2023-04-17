using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Viber.Bot.NetCore.Infrastructure;
using Viber.Bot.NetCore.Models;
using Viber.Bot.NetCore.RestApi;
using Web.Services.Interfaces;

namespace Web.Controllers
{
    [Route("bot")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IViberService _viberService;
        private readonly IViberBotApi _viberBotApi;

        public BotController(IViberService viberService, IViberBotApi viberBotApi)
        {
            _viberService = viberService;
            _viberBotApi = viberBotApi;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Refit.ApiResponse<ViberWebHook.ViberWebHookResponse> response = await _viberBotApi.SetWebHookAsync(new ViberWebHook.WebHookRequest("https://591f-176-119-235-147.ngrok-free.app/bot"));

            return response.Content.Status == ViberErrorCode.Ok ? Ok("Viber-bot is active") : BadRequest(response.Content.StatusMessage);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ViberCallbackData update, CancellationToken cancellationToken)
        {
            if (update.Event is not (ViberEventType.Message or ViberEventType.Subscribed))
            {
                return Ok();
            }

            await _viberService.HandleMessage(update, cancellationToken);
            return Ok();
        }
    }
}