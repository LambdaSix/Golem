using System;
using System.Runtime.Serialization;
using Golem.Server.Enumerations;

namespace Golem.Server.Interfaces
{
    public interface IMobile : IEntity, IComparable<IMobile>, ISerializable, ISpawnable
    {
        void SendMessage(string messageStr);
        AccessLevel AccessLevel { get; set; }
    }
}