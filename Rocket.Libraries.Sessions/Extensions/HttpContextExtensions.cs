using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Exceptions;
using Rocket.Libraries.Sessions.Models;

namespace Rocket.Libraries.Sessions.Extensions
{
    public static class HttpContextExtensions
    {
        public static Session GetSession(this IHttpContextAccessor httpContextAccessor)
        {
            FailIfSessionInformationHeaderMissing(httpContextAccessor);
            var sessionInformation = httpContextAccessor.HttpContext.Request.Headers[HeaderNameConstants.SessionInformation].ToString();
            FailIfSessionInformationHeaderEmpty(sessionInformation);
            return JsonConvert.DeserializeObject<Session>(sessionInformation);
        }

        private static void FailIfSessionInformationHeaderMissing(IHttpContextAccessor httpContextAccessor)
        {
            var hasHeader = httpContextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderNameConstants.SessionInformation);
            if (hasHeader == false)
            {
                throw new SessionInformationHeaderMissingException();
            }
        }

        private static void FailIfSessionInformationHeaderEmpty(string sessionInformation)
        {
            if (string.IsNullOrEmpty(sessionInformation))
            {
                throw new SessionInformationEmptyException();
            }
        }
    }
}