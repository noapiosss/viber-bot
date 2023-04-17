using System.Threading;
using System.Threading.Tasks;
using Viber.Bot;

namespace Web.Services.Interfaces
{
    public interface IViberService
    {
        Task HandleMessage(CallbackData update, CancellationToken cancellationToken);
    }
}