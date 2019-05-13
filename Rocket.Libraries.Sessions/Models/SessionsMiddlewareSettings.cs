using System.Collections.Generic;

namespace Rocket.Libraries.Sessions.Models
{
    public class SessionsMiddlewareSettings
    {
        public string SessionsServerBaseUri { get; set; }
        public List<string> PersistantKeys { get; set; }
    }
}