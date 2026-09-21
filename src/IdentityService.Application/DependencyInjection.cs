using FluentValidation;
using IdentityService.Application.Features.Users.Interfaces;
using IdentityService.Application.Features.Users.Services;
using Microsoft.Extensions.DependencyInjection;


namespace IdentityService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
    this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly);

            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
