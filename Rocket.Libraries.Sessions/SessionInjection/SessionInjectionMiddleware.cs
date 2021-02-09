using Microsoft.AspNetCore.Http;
using Rocket.Libraries.Sessions.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.SessionInjection
{
    public class SessionInjectionMiddleware
    {
        private readonly RequestDelegate requestDelegate;

        public SessionInjectionMiddleware(
            RequestDelegate requestDelegate)
        {
            this.requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(
            HttpContext httpContext,
            ISessionInjector sessionInjector
            )
        {
            var sessionSerialized = sessionInjector.GetSessionSerialized;
            if (string.IsNullOrEmpty(sessionSerialized))
            {
                throw new Exception("No session was received for injection");
            }
            httpContext.Request.Headers.Add(
                HeaderNameConstants.SessionInformation,
                sessionSerialized);
            await requestDelegate(httpContext);
        }
    }
}