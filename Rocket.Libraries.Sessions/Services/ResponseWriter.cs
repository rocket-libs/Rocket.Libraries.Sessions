using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Models;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    internal class ResponseWriter
    {
        public async Task WriteSuccess<TResponse>(HttpContext httpContext, TResponse response)
        {
            var responseObj = new ResponseObject<TResponse>
            {
                Code = 1,
                Payload = response
            };

            await WriteAsync(httpContext, responseObj, 200);
        }

        public async Task WriteAuthenticationErrorAsync(HttpContext httpContext, string errorMessage)
        {
            await WriteErrorAsync(httpContext, errorMessage, 401);
        }

        public async Task WriteGenericErrorAsync(HttpContext httpContext, string errorMessage)
        {
            await WriteErrorAsync(httpContext, errorMessage, 500);
        }

        private async Task WriteErrorAsync(HttpContext httpContext, string errorMessage, int httpStatusCode)
        {
            LogUtility.Error(errorMessage);
            var responseObj = new ResponseObject<string>
            {
                Code = 2,
                Payload = errorMessage
            };
            await WriteAsync(httpContext, responseObj, httpStatusCode);
        }

        private async Task WriteAsync<TResponse>(HttpContext httpContext, ResponseObject<TResponse> response, int httpStatusCode)
        {
            var stringified = JsonConvert.SerializeObject(response);
            httpContext.Response.StatusCode = httpStatusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(stringified);
        }
    }
}