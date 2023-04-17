using System;
using Domain.Database;
using Domain.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class DomainExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services,
            Action<IServiceProvider, DbContextOptionsBuilder> dbOptionsAction)
        {
            return services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetWalksByImeiQuery).Assembly))
                .AddDbContext<TrackDbContext>(dbOptionsAction);
        }
    }
}