using System.Threading;
using System.Threading.Tasks;
using Viber.Bot.NetCore.Models;

namespace Web.Services.Interfaces
{
    public interface IViberService
    {
        Task HandleMessage(ViberCallbackData update, CancellationToken cancellationToken);
    }
}