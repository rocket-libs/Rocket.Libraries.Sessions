using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;

namespace Rocket.Libraries.Sessions.Services
{
    internal class HeaderSessionReader : ISessionReader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SessionsMiddlewareSettings _sessionsMiddlewareSetting;

        public HeaderSessionReader(IHttpClientFactory httpClientFactory, SessionsMiddlewareSettings sessionsMiddlewareSetting)
        {
            _httpClientFactory = httpClientFactory;
            _sessionsMiddlewareSetting = sessionsMiddlewareSetting;
        }

        public async Task<string> ReadAsync(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            var sessionKey = GetSessionKey(httpContext);
            var hasSessionKey = !string.IsNullOrEmpty(sessionKey);
            if (hasSessionKey)
            {
                var errorMessage = GetRequestValidationErrorsIfAny();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    throw new Exception(errorMessage);
                }
                else
                {
                    var sessionInformation = await new SessionFetcher(_httpClientFactory, _sessionsMiddlewareSetting).GetSessionAsync(sessionKey);

                    if (new SessionValidator().IsValidSession(sessionKey, sessionInformation))
                    {
                        return sessionInformation.Value;
                    }
                    else
                    {
                        throw new Exception($"Your session has expired or is invalid.");
                    }
                }
            }
            else
            {
                throw new Exception($"You are currently not authenticated. Please sign in to continue.");
            }
        }

        public bool UseThisReader(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            return true;
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

        private string GetRequestValidationErrorsIfAny()
        {
            if (_httpClientFactory == null)
            {
                return $"HttpClientFactory not injected. Cannot fetch session";
            }
            else if (_sessionsMiddlewareSetting == null)
            {
                return $"Sessions middleware settings not supplied. Cannot figure out how to connect to the sessions server";
            }
            else if (string.IsNullOrEmpty(_sessionsMiddlewareSetting.SessionsServerBaseUri))
            {
                return $"Sessions 'SessionsServerBaseUri' not specified. Please supply this value.";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}