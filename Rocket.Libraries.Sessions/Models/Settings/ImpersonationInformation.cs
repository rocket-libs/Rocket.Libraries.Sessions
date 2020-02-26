using System.Collections.Generic;

namespace Rocket.Libraries.Sessions.Models.Settings
{
    public class ImpersonationInformation
    {
        public List<ImpersonationSession> ImpersonationSessions { get; set; }
        public List<EndpointPath> EndpointPaths { get; set; }
    }
}