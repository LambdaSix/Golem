using System;
using System.Net.Sockets;

namespace Golem.Server.Network
{
    public interface IConnection
    {
        event EventHandler<LineReceivedEventArgs> LineReceived;
        event EventHandler Closed;
        Socket Socket { get; }

        void Write(string value);
        void WriteLine(string value);
        void Close();
        void Flush();
        void Fill();
        void SetEcho(bool echo);
    }
}