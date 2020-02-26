using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    internal class SessionFetcher
    {
        private SessionsMiddlewareSettings _sessionsMiddlewareSettings;
        private IHttpClientFactory _httpClientFactory;

        public SessionFetcher(IHttpClientFactory httpClientFactory, SessionsMiddlewareSettings sessionsMiddlewareSettings)
        {
            _sessionsMiddlewareSettings = sessionsMiddlewareSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SessionInformation> GetSessionAsync(string sessionKey)
        {
            var trimmedSessionKey = sessionKey.Trim();
            var noKey = string.IsNullOrEmpty(trimmedSessionKey);
            WarnIfKeyMissing(noKey);
            if (noKey)
            {
                return default;
            }
            else
            {
                return await ReadRepositoryAsync(trimmedSessionKey);
            }
        }

        private void WarnIfKeyMissing(bool noKey)
        {
            if (noKey)
            {
                LogUtility.Warn($"Ignored request without a session key");
            }
            else
            {
                return;
            }
        }

        private async Task<SessionInformation> ReadRepositoryAsync(string sessionKey)
        {
            var sessionsServerUrlMinusKey = $"{_sessionsMiddlewareSettings.SessionsServerBaseUri}api/v1/repository/get?key=";
            var sessionsServerUrlIncludingKey = $"{sessionsServerUrlMinusKey}{sessionKey}";
            LogUtility.Debug($"Sessions Server Call Url: {sessionsServerUrlIncludingKey}");
            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                sessionsServerUrlIncludingKey
            );

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var sessionServerResponse = JsonConvert.DeserializeObject<ResponseObject<SessionInformation>>(responseString);
                return sessionServerResponse.Payload;
            }
            else
            {
                throw new Exception($"Error occured calling the session server. \n\tResponse Code: {response.StatusCode}\n\tMessage: {(await response.Content?.ReadAsStringAsync())}");
            }
        }
    }
}