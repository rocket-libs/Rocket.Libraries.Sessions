using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    public class PersistantSessionsReader : IPersistantSessionsReader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SessionsMiddlewareSettings _sessionsMiddlewareSettings;
        private List<Session> _persistantSessions;

        public PersistantSessionsReader(IHttpClientFactory httpClientFactory, IOptions<SessionsMiddlewareSettings> sessionsMiddlewareSettings)
        {
            _httpClientFactory = httpClientFactory;
            _sessionsMiddlewareSettings = sessionsMiddlewareSettings.Value;
        }

        public async Task<List<Session>> GetPersistantTokensAsync()
        {
            if (_persistantSessions?.Count > 0)
            {
                return _persistantSessions;
            }
            else
            {
                return await FetchSessionsAsync();
            }
        }

        private async Task<List<Session>> FetchSessionsAsync()
        {
            var persistantKeysMissing = _sessionsMiddlewareSettings.PersistantKeys?.Count == 0;
            if (persistantKeysMissing)
            {
                return null;
            }
            else
            {
                _persistantSessions = new List<Session>();
                var sessionFetcher = new SessionFetcher(_httpClientFactory, _sessionsMiddlewareSettings);
                var sessionValidator = new SessionValidator();
                foreach (var persistantKey in _sessionsMiddlewareSettings.PersistantKeys)
                {
                    var sessionInformation = await sessionFetcher.GetSessionAsync(persistantKey);
                    if (sessionValidator.IsValidSession(persistantKey, sessionInformation))
                    {
                        _persistantSessions.Add(JsonConvert.DeserializeObject<Session>(sessionInformation.Value));
                    }
                }
                return _persistantSessions;
            }
        }
    }
}