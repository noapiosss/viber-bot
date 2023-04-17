using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Viber.Bot;
using Web.Services.Interfaces;

namespace Web.Controllers
{
    public class BotController : ControllerBase
    {
        private readonly IViberService _viberService;
        private readonly IViberBotClient _viberBotClient;

        public BotController(IViberService viberService, IViberBotClient viberBotClient)
        {
            _viberService = viberService;
            _viberBotClient = viberBotClient;
        }

        [HttpPost("/bot")]
        public async Task<IActionResult> Post([FromBody] CallbackData update, CancellationToken cancellationToken)
        {
            if (update.Event == EventType.Webhook)
            {
                return Ok();
            }
            if (update.Event is not (EventType.Message or EventType.Subscribed))
            {
                return Ok();
            }

            await _viberService.HandleMessage(update, cancellationToken);
            return Ok();
        }
    }
}