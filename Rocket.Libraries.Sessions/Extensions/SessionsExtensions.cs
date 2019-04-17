using Microsoft.Extensions.DependencyInjection;
using Rocket.Libraries.Sessions.Services.SessionProvider;

namespace Rocket.Libraries.Sessions.Extensions
{
    public static class SessionsExtensions
    {
        public static void ConfigureSessions(this IServiceCollection services)
        {
            services.AddScoped<IRocketSessionCache, RocketSessionCache>();
        }
    }
}