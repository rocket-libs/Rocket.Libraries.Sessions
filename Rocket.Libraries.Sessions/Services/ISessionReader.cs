using Microsoft.AspNetCore.Http;
using Rocket.Libraries.Sessions.Models;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    internal interface ISessionReader
    {
        bool UseThisReader(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings);

        Task<string> ReadAsync(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings);
    }
}