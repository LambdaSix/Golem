using System.Net.Sockets;

namespace Golem.Server.Network
{
    public interface ConnectionHandler
    {
        void Handle(Socket socket);
    }
}