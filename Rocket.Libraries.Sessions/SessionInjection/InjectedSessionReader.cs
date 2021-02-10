using Microsoft.AspNetCore.Http;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;
using Rocket.Libraries.Sessions.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.SessionInjection
{
    public class InjectedSessionReader : ISessionReader
    {
        public async Task<string> ReadAsync(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            return await Task.Run(() => httpContext.Request.Headers[HeaderNameConstants.SessionInformation]);
        }

        public bool UseThisReader(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            return httpContext.Request.Headers.ContainsKey(HeaderNameConstants.SessionInformation);
        }
    }
}