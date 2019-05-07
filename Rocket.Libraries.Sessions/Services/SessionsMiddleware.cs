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
                    var sessionInformation = await GetSessionAsync(sessionKey);

                    if (IsValidSession(sessionKey, sessionInformation))
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

        private bool IsValidSession(string sessionKey, SessionInformation sessionInformation)
        {
            var sessionInformationIsNull = sessionInformation == null;
            if (sessionInformationIsNull)
            {
                LogUtility.Debug($"Could not find session with key '{sessionKey}'");
                return false;
            }
            else
            {
                var sessionDataMissing = (string.IsNullOrEmpty(sessionInformation.Value) || string.IsNullOrEmpty(sessionInformation.Key));
                if (sessionDataMissing)
                {
                    LogUtility.Debug($"Session data is incomplete");
                    return false;
                }
                else
                {
                    var sessionKeyMismatch = sessionDataMissing == false && sessionInformation.Key.Equals(sessionKey, StringComparison.InvariantCultureIgnoreCase) == false;
                    if (sessionKeyMismatch)
                    {
                        LogUtility.Debug($"Supplied session key does not match with key from server");
                        return false;
                    }
                }
            }
            return true;
        }

        private async Task<SessionInformation> GetSessionAsync(string sessionKey)
        {
            var trimmedSessionKey = sessionKey.Trim();
            var noKey = string.IsNullOrEmpty(trimmedSessionKey);
            WarnIfKeyMissing(noKey);
            if (noKey)
            {
                return default;
            }
            else
            {
                return await ReadRepositoryAsync(trimmedSessionKey);
            }
        }

        private async Task<SessionInformation> ReadRepositoryAsync(string sessionKey)
        {
            var sessionsServerUrlMinusKey = $"{_sessionManagerSettings.SessionsServerBaseUri}api/v1/repository/get?key=";
            var sessionsServerUrlIncludingKey = $"{sessionsServerUrlMinusKey}{sessionKey}";
            LogUtility.Debug($"Sessions Server Call Url: {sessionsServerUrlIncludingKey}");
            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                sessionsServerUrlIncludingKey
            );

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var sessionServerResponse = JsonConvert.DeserializeObject<ResponseObject<SessionInformation>>(responseString);
                return sessionServerResponse.Payload;
            }
            else
            {
                throw new Exception($"Error occured calling the session server. \n\tResponse Code: {response.StatusCode}\n\tMessage: {(await response.Content?.ReadAsStringAsync())}");
            }
        }

        private void WarnIfKeyMissing(bool noKey)
        {
            if (noKey)
            {
                LogUtility.Warn($"Ignored request without a session key");
            }
            else
            {
                return;
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