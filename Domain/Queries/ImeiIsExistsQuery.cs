using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Domain.Queries
{
    public class ImeiExistsQuery : IRequest<ImeiExistsQueryResult>
    {
        public string Imei { get; set; }
    }

    public class ImeiExistsQueryResult
    {
        public bool EmeiExists { get; set; }
    }

    internal class ImeiExistsQueryHandler : BaseHandler<ImeiExistsQuery, ImeiExistsQueryResult>
    {
        private readonly TrackDbContext _dbContext;

        public ImeiExistsQueryHandler(TrackDbContext dbContext, ILogger<ImeiExistsQuery> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ImeiExistsQueryResult> HandleInternal(ImeiExistsQuery request, CancellationToken cancellationToken)
        {
            return new()
            {
                EmeiExists = await _dbContext.TrackLocations.AnyAsync(tl => tl.Imei == request.Imei, cancellationToken)
            };
        }
    }
}