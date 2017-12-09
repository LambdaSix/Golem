using System;
using System.Net.Sockets;

namespace Golem.Server.Network
{
    class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(Socket socket)
        {
            Socket = socket;
        }

        public Socket Socket { get; private set; }
    }
}