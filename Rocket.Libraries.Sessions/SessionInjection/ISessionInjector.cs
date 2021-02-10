using Rocket.Libraries.Sessions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Sessions.SessionInjection
{
    public interface ISessionInjector<TIdentifier>
    {
        void InjectSession(TypeableSession<TIdentifier> session);
    }
}