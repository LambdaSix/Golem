using System;
using System.Runtime.Serialization;
using Golem.Server.Enumerations;

namespace Golem.Server.Interfaces
{
    public interface IMobile : IComparable<IMobile>
    {
        int Serial { get; set; }

        void SendMessage(string messageStr);
        AccessLevel AccessLevel { get; set; }
    }
}