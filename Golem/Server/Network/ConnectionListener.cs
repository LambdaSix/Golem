using System;
using System.Net;
using System.Net.Sockets;

namespace Golem.Server.Network
{
    public class ConnectionListener : IConnectionListener, IDisposable
    {
        private Socket listenSocket = null;
        private bool isRunning = false;
        private bool isDisposed = false;

        public ConnectionHandler ConnectionHandler { get; set; }
        
        private int Port { get; }

        public ConnectionListener(int port)
        {
            Port = port;
        }

        /// <inheritdoc />
        public void Start()
        {
            if (isRunning)
                throw new InvalidOperationException("ConnectionListener already running");

            CheckDisposed();

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            listenSocket.Listen(10);
            listenSocket.BeginAccept(OnConnectionAccepted, null);
        }

        /// <inheritdoc />
        public void Stop()
        {
            listenSocket.Close();
            isRunning = false;
        }

        private void OnNewConnection(Socket socket)
        {
            if (ConnectionHandler != null)
                ConnectionHandler.Handle(socket);
            else
                socket.Close();
        }

        private void OnConnectionAccepted(IAsyncResult result)
        {
            try
            {
                try
                {
                    var socket = listenSocket.EndAccept(result);
                    OnNewConnection(socket);
                }
                finally
                {
                    listenSocket.BeginAccept(OnConnectionAccepted, null);
                }
            }
            catch
            {
                GolemServer.Current.Log(LogType.Warning, "OnConnectionAccepted error (probably on shutdown");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            CheckDisposed();

            Stop();
            listenSocket.Dispose();
            isDisposed = true;
        }

        private void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException("ConnectionListener is disposed");
        }
    }
}