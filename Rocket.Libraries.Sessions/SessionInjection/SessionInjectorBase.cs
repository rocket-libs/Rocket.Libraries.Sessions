using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Constants;
using Rocket.Libraries.Sessions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Sessions.SessionInjection
{
    public class SessionInjectorBase<TIdentifier> : ISessionInjector<TIdentifier>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SessionInjectorBase(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void InjectSession(TypeableSession<TIdentifier> session)
        {
            httpContextAccessor.HttpContext.Request.Headers.Add(HeaderNameConstants.SessionInformation, JsonConvert.SerializeObject(session));
        }
    }
}