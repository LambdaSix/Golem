using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Golem.Server.Network
{
    public class ConnectionMonitor : IConnectionMonitor
    {
        private List<IConnection> Connections { get; } = new List<IConnection>();
        private Dictionary<Socket, IConnection> ConnectionMap { get; } = new Dictionary<Socket, IConnection>();

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            var connection = sender as Connection;
            if (connection == null)
                return;

            Connections.Remove(connection);
        }

        public void RegisterConnection(IConnection connection)
        {
            connection.Closed += OnConnectionClosed;
            Connections.Add(connection);
            Update();
        }

        public void Update()
        {
            var dict = Connections.ToDictionary(c => c.Socket);
            var read = new List<Socket>(Connections.Select(c => c.Socket));
            var write = new List<Socket>(Connections.Select(c => c.Socket));

            if (read.Count > 0 || write.Count > 0)
                Socket.Select(read, write, null, 0);

            foreach (var socket in write)
                dict[socket]?.Flush();

            foreach (var socket in read)
                dict[socket].Fill();
        }
    }
}