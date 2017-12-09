using Golem.Server.Network;

namespace Golem.Server
{
    public interface IConnectionMonitor : IUpdatable
    {
        void RegisterConnection(IConnection connection);
    }
}