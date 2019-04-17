using Rocket.Libraries.Sessions.Models;

namespace Rocket.Libraries.Sessions.Services.SessionProvider
{
    public class RocketSessionCache : IRocketSessionCache
    {
        public Session Session { get; set; }
    }
}