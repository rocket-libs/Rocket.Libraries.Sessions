using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;
using System;
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

        public SessionsMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, SessionsMiddlewareSettings sessionsMiddlewareSettings)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _sessionManagerSettings = sessionsMiddlewareSettings;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var sessionKey = GetSessionKey(httpContext);
            var hasSessionKey = !string.IsNullOrEmpty(sessionKey);
            if (hasSessionKey)
            {
                var errorMessage = GetRequestValidationErrorsIfAny();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    await _responseWriter.WriteGenericErrorAsync(httpContext, errorMessage);
                    return;
                }
                else
                {
                    var sessionInformation = await new SessionFetcher(_httpClientFactory, _sessionManagerSettings).GetSessionAsync(sessionKey);

                    if (new SessionValidator().IsValidSession(sessionKey, sessionInformation))
                    {
                        httpContext.Request.Headers[HeaderNameConstants.SessionInformation] = sessionInformation.Value;
                        LogUtility.Debug($"Session for key '{sessionKey}' verified and appended to headers.");
                    }
                    else
                    {
                        await _responseWriter.WriteAuthenticationErrorAsync(httpContext, $"Your session has expired or is invalid.");
                        return;
                    }
                }
            }
            await _next.Invoke(httpContext);
        }

        private string GetRequestValidationErrorsIfAny()
        {
            if (_httpClientFactory == null)
            {
                return $"HttpClientFactory not injected. Cannot fetch session";
            }
            else if (_sessionManagerSettings == null)
            {
                return $"Sessions middleware settings not supplied. Cannot figure out how to connect to the sessions server";
            }
            else if (string.IsNullOrEmpty(_sessionManagerSettings.SessionsServerBaseUri))
            {
                return $"Sessions 'SessionsServerBaseUri' not specified. Please supply this value.";
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetSessionKey(HttpContext httpContext)
        {
            var containsHeader = httpContext.Request.Headers.ContainsKey(HeaderNameConstants.SessionKey);
            if (containsHeader)
            {
                return httpContext.Request.Headers[HeaderNameConstants.SessionKey].First();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}