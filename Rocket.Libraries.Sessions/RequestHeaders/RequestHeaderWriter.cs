using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Rocket.Libraries.Sessions.RequestHeaders
{
    public class RequestHeaderWriter : IRequestHeaderWriter
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public RequestHeaderWriter(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Write(string key, object value)
        {
            var keyNameNotProvided = string.IsNullOrEmpty(key);
            if (keyNameNotProvided)
            {
                throw new Exception("No value for key was provided");
            }
            if (httpContextAccessor.HttpContext.Request.Headers.ContainsKey(key))
            {
                httpContextAccessor.HttpContext.Request.Headers.Remove(key);
            }

            var stringValues = new StringValues(value == null ? string.Empty : value.ToString());
            httpContextAccessor.HttpContext.Request.Headers.Add(key, stringValues);
        }
    }
}