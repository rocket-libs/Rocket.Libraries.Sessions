using Microsoft.AspNetCore.Http;
using System;

namespace Rocket.Libraries.Sessions.RequestHeaders
{
    public class RequestHeaderReader : IRequestHeaderReader
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public RequestHeaderReader(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string ReadOrDefault(string key)
        {
            var value = httpContextAccessor.HttpContext.Request.Headers[key];
            return value;
        }

        public string Read(string key)
        {
            var keyNotFound = httpContextAccessor.HttpContext.Request.Headers.ContainsKey(key) == false;
            if (keyNotFound)
            {
                throw new Exception($"Couldn't find header with key '{key}'");
            }
            return ReadOrDefault(key);
        }
    }
}