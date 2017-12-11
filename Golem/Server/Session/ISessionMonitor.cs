using System.Collections.Generic;
using Golem.Game.Mobiles;

namespace Golem.Server.Session
{
    public interface ISessionMonitor : IEnumerable<ISession>
    {
        void RegisterSession(ISession session);
        ISession FindSessionForPlayer(IPlayer player);
    }
}