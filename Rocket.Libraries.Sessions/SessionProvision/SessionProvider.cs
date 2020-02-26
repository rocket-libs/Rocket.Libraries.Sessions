using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;
using Rocket.Libraries.Sessions.RequestHeaders;
using System;

namespace Rocket.Libraries.Sessions.SessionProvision
{
    public class SessionProvider<TIdentifier> : ISessionProvider<TIdentifier>
    {
        private readonly IRequestHeaderReader requestHeaderReader;

        public SessionProvider(
            IRequestHeaderReader requestHeaderReader)
        {
            this.requestHeaderReader = requestHeaderReader;
        }

        public TypeableSession<TIdentifier> Session
        {
            get
            {
                var sessionInformationString = requestHeaderReader.Read(HeaderNameConstants.SessionInformation);
                var sessionInformationNotProvided = string.IsNullOrEmpty(sessionInformationString);
                if (sessionInformationNotProvided)
                {
                    throw new Exception($"No session information was found");
                }

                var session = JsonConvert.DeserializeObject<TypeableSession<TIdentifier>>(sessionInformationString);
                var userSessionNotDetermined = session == null;
                if (userSessionNotDetermined)
                {
                    throw new Exception("User session could not be determined");
                }
                return session;
            }
        }
    }
}