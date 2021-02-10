using Microsoft.AspNetCore.Http;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;
using Rocket.Libraries.Sessions.SessionInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    public class SessionsMiddleware
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly RequestDelegate _next;

        private readonly ResponseWriter _responseWriter = new ResponseWriter();

        private readonly SessionsMiddlewareSettings _sessionManagerSettings;

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
            try
            {
                var sessionInfoRequired = NotOptionsCall(httpContext);
                if (sessionInfoRequired)
                {
                    await InjectSessionInformation(httpContext);
                }
                await _next.Invoke(httpContext);
            }
            catch (Exception e)
            {
                await _responseWriter.WriteAuthenticationErrorAsync(httpContext, e.Message);
                return;
            }
        }

        private ISessionReader GetSessionReaderToUse(HttpContext httpContext)
        {
            foreach (var sessionReader in _sessionReaders)
            {
                if (sessionReader.UseThisReader(httpContext, _sessionManagerSettings))
                {
                    return sessionReader;
                }
            }
            return null;
        }

        private void InitializeSessionReaders()
        {
            _sessionReaders = new List<ISessionReader>
            {
                new Impersonator(),
                new InjectedSessionReader(),
                new HeaderSessionReader(_httpClientFactory,_sessionManagerSettings),
            };
        }

        private async Task InjectSessionInformation(HttpContext httpContext)
        {
            var sessionReader = GetSessionReaderToUse(httpContext);
            if (sessionReader == null)
            {
                throw new Exception($"Could not determine what session reader to use for path {httpContext.Request.Path.Value}");
            }
            LogUtility.Debug($"Using session reader '{sessionReader.GetType().Name}'");
            httpContext.Request.Headers[HeaderNameConstants.SessionInformation] = await sessionReader.ReadAsync(httpContext, _sessionManagerSettings);
            LogUtility.Debug($"Session appended to headers.");
        }

        private bool NotOptionsCall(HttpContext httpContext)
        {
            var isOptions = httpContext.Request.Method.Equals("Options", StringComparison.InvariantCultureIgnoreCase);
            var notOptions = isOptions == false;
            return notOptions;
        }
    }
}