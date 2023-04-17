using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalksController : ControllerBase
    {
        private readonly ILogger<WalksController> _logger;
        private readonly IMediator _mediator;

        public WalksController(ILogger<WalksController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("{imei}")]
        public async Task<IEnumerable<Walk>> Get([FromRoute] string imei, CancellationToken cancellationToken)
        {
            GetWalksByImeiQuery request = new() { Imei = imei };
            GetWalksByImeiQueryResult result = await _mediator.Send(request, cancellationToken);

            return result.Walks.OrderByDescending(w => w.Distance);
        }
    }
}