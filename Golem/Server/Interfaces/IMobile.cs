using System;
using System.Runtime.Serialization;
using Golem.Common.Enumerations;

namespace Golem.Common.Interfaces
{
    public interface IMobile : IEntity, IComparable<IMobile>, ISerializable, ISpawnable
    {
        void SendMessage(string messageStr);
        AccessLevel AccessLevel { get; set; }
    }
}