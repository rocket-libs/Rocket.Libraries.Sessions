using Rocket.Libraries.Sessions.Models;

namespace Rocket.Libraries.Sessions.Services.SessionProvider
{
    public interface IRocketSessionCache
    {
        Session Session { get; set; }
    }
}