using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Sessions.SessionInjection
{
    public static class SessionInjectionExtensions
    {
        public static IServiceCollection RegisterSessionInjector
            <TSessionInjector>(this IServiceCollection services)
            where TSessionInjector : class, ISessionInjector
        {
            return services
                     .AddScoped<ISessionInjector, TSessionInjector>();
        }

        public static IApplicationBuilder UseSessionInjection(
                    this IApplicationBuilder builder)

        {
            return builder.UseMiddleware<SessionInjectionMiddleware>();
        }
    }
}