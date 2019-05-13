using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.Libraries.Sessions.Models;

namespace Rocket.Libraries.Sessions.Services
{
    public interface IPersistantSessionsReader
    {
        Task<List<Session>> GetPersistantTokensAsync();
    }
}