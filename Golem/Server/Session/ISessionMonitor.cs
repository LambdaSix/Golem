using System.Collections.Generic;

namespace Golem.Server.Session
{
    public interface ISessionMonitor : IEnumerable<ISession>
    {
        void RegisterSession(ISession session);
    }
}