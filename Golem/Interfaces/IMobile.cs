using System;
using System.Runtime.Serialization;

namespace Golem.Common.Interfaces
{
    public interface IMobile : IEntity, IComparable<IMobile>, ISerializable, ISpawnable
    {
        
    }
}