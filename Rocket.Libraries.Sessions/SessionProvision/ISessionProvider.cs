using Rocket.Libraries.Sessions.Models;

namespace Rocket.Libraries.Sessions.SessionProvision
{
    public interface ISessionProvider<TIdentifier>
    {
        TypeableSession<TIdentifier> Session { get; }
    }
}