using System;

namespace Golem.Server.Network
{
    public class LineReceivedEventArgs : EventArgs
    {
        public LineReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; private set; }
    }
}