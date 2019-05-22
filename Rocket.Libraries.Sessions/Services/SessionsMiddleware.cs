using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    public class SessionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SessionsMiddlewareSettings _sessionManagerSettings;
        private readonly ResponseWriter _responseWriter = new ResponseWriter();
        private List<ISessionReader> _sessionReaders;

        public SessionsMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, SessionsMiddlewareSettings sessionsMiddlewareSettings)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _sessionManagerSettings = sessionsMiddlewareSettings;
            InitializeSessionReaders();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            LogUtility.Debug($"Processing path: {httpContext.Request.Path.Value}");
            foreach (var sessionReader in _sessionReaders)
            {
                if (sessionReader.UseThisReader(httpContext, _sessionManagerSettings))
                {
                    try
                    {
                        LogUtility.Debug($"Using session reader '{sessionReader.GetType().Name}'");
                        httpContext.Request.Headers[HeaderNameConstants.SessionInformation] = await sessionReader.ReadAsync(httpContext, _sessionManagerSettings);
                        LogUtility.Debug($"Session appended to headers.");
                        await _next.Invoke(httpContext);
                        return;
                    }
                    catch (Exception e)
                    {
                        await _responseWriter.WriteAuthenticationErrorAsync(httpContext, e.Message);
                        return;
                    }
                }
            }
            await _responseWriter.WriteAuthenticationErrorAsync(httpContext, "Could not determine what session reader to use");
            return;
        }

        private void InitializeSessionReaders()
        {
            _sessionReaders = new List<ISessionReader>
            {
                new Impersonator(),
                new HeaderSessionReader(_httpClientFactory,_sessionManagerSettings),
            };
        }
    }
}