using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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

        public SessionsMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IOptions<SessionsMiddlewareSettings> sessionManagerSettingsOptions)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _sessionManagerSettings = sessionManagerSettingsOptions.Value;
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
                    httpContext.Response.StatusCode = 500;
                    await httpContext.Response.WriteAsync(errorMessage);
                    return;
                }
                else
                {
                    var sessionInformation = await GetSessionAsync(sessionKey);

                    if (IsValidSession(sessionKey, sessionInformation))
                    {
                        httpContext.Response.StatusCode = 401;
                        await httpContext.Response.WriteAsync("Session has expired or does not exist");
                        return;
                    }
                    else
                    {
                        httpContext.Request.Headers.Add(HeaderNameConstants.SessionInformation, sessionInformation.Value);
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
                return $"Sessions middleware settings not configured. Please add to section 'SessionsMiddleware' in your appsettings.json";
            }
            else if (string.IsNullOrEmpty(_sessionManagerSettings.SessionsServerBaseUri))
            {
                return $"Sessions server base uri not specified. Please add to section 'SessionsMiddleware' in your appsettings.json";
            }
            else
            {
                return string.Empty;
            }
        }

        private bool IsValidSession(string sessionKey, SessionInformation sessionInformation)
        {
            var sessionInformationIsNull = sessionInformation == null;
            var sessionDataMissing = sessionInformationIsNull == false && (string.IsNullOrEmpty(sessionInformation.Value) || string.IsNullOrEmpty(sessionInformation.Key));
            var sessionKeyMismatch = sessionDataMissing == false && sessionInformation.Key.Equals(sessionKey, StringComparison.InvariantCultureIgnoreCase) == false;

            if (sessionInformationIsNull || sessionDataMissing || sessionKeyMismatch)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task<SessionInformation> GetSessionAsync(string sessionKey)
        {
            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_sessionManagerSettings.SessionsServerBaseUri}api/v1/get?key={sessionKey}"
            );

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(requestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            var sessionInformation = JsonConvert.DeserializeObject<SessionInformation>(responseString);
            return sessionInformation;
        }

        private string GetSessionKey(HttpContext httpContext)
        {
            var containsHeader = httpContext.Request.Headers.ContainsKey(HeaderNameConstants.SessionName);
            if (containsHeader)
            {
                return httpContext.Request.Headers[HeaderNameConstants.SessionName].First();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}