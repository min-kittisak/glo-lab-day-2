using IdentityService.Application.Features.Users.Interfaces;
using IdentityService.Infrastructure.Persistence.Context;
using IdentityService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("WorkshopConnection")
                ?? throw new InvalidOperationException(
                    "ไม่พบ Connection String ชื่อ DefaultConnection");

            services.AddDbContext<IdentityServiceDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });


            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
