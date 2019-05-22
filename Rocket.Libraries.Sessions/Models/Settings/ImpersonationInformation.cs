﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Sessions.Models.Settings
{
    public class ImpersonationInformation
    {
        public List<ImpersonationSession> ImpersonationSessions { get; set; }
        public List<EndpointPath> EndpointPaths { get; set; }
    }
}