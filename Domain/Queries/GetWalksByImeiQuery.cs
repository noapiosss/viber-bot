using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Contracts.Models;
using Domain.Base;
using Domain.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Domain.Queries
{
    public class GetWalksByImeiQuery : IRequest<GetWalksByImeiQueryResult>
    {
        public string Imei { get; set; }
    }

    public class GetWalksByImeiQueryResult
    {
        public ICollection<Walk> Walks { get; set; }
    }

    internal class GetWalksByImeiQueryHandler : BaseHandler<GetWalksByImeiQuery, GetWalksByImeiQueryResult>
    {
        private readonly TrackDbContext _dbContext;

        public GetWalksByImeiQueryHandler(TrackDbContext dbContext, ILogger<GetWalksByImeiQuery> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<GetWalksByImeiQueryResult> HandleInternal(GetWalksByImeiQuery request, CancellationToken cancellationToken)
        {
            if (!await _dbContext.TrackLocations.AnyAsync(tl => tl.Imei == request.Imei, cancellationToken))
            {
                return null;
            }

            List<TrackLocation> trackLocations = await _dbContext.TrackLocations
                .Where(tl => tl.Imei == request.Imei)
                .OrderByDescending(tl => tl.DateTrack)
                .ToListAsync(cancellationToken);

            List<Walk> walks = new();

            List<TrackLocation> walkLocations = new()
            {
                trackLocations[0]
            };

            for (int i = 0; i < trackLocations.Count - 1; ++i)
            {
                if (trackLocations[i].DateTrack - trackLocations[i + 1].DateTrack > TimeSpan.FromMinutes(30))
                {
                    Walk walk = new(walkLocations);

                    // walk with 0 distance isn't walk
                    // also, probably, if walk speed is over 10km/h it becomes a ride
                    if (walk.Distance > 0 && walk.Distance / walk.Duration.TotalHours <= 10)
                    {
                        walks.Add(walk);
                    }
                    walkLocations.Clear();
                }

                walkLocations.Add(trackLocations[i + 1]);
            }

            walks.Add(new(walkLocations));

            return new()
            {
                Walks = walks.OrderByDescending(w => w.Distance).ToList()
            };
        }
    }
}