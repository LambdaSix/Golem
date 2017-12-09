using System.Collections.Generic;
using System.Linq;
using Capsicum;
using Capsicum.Interfaces;
using Golem.Server.Interfaces;
using Golem.Server.Network;

namespace Golem.Game.Core
{
    /// <summary>
    /// Placeholder component containing everything for a Thing
    /// </summary>
    public class ThingComponent : IComponent
    {
        public int VNum { get; set; }

        /// <summary>
        /// Is this entity contained inside another?
        /// </summary>
        public Entity Inside { get; set; } = null;

        /// <summary>
        /// What this entity contains
        /// </summary>
        public LinkedList<Entity> ContainedEntities { get; set; } = new LinkedList<Entity>();

        /// <summary>
        /// Events for this entity
        /// </summary>
        public LinkedList<Entity> Events { get; set; }
    }

    public class NetworkComponent : IComponent
    {
        public INetState NetState { get; set; }
    }

    public class AreaComponent : IComponent
    {
        public int VNum { get; set; }

        public string Type { get; set; }
    }

    public static class Thing
    {
        public static int thing_made_count = 0;
        public static Pool thing_pool = new Pool();

        public static Entity new_entity()
        {
            thing_made_count++;

            var entity = thing_pool.CreateEntity();
            entity.AddComponent(new ThingComponent());
            return entity;
        }

        /// <summary>
        /// Free an entity for reuse.
        /// </summary>
        /// <param name="entity"></param>
        public static void free_entity(Entity entity)
        {
            entity.Destroy();
        }

        public static Entity find_thing_vnum(int vnum)
        {
            return thing_pool.GetAllEntities().SingleOrDefault(s => s.GetComponent<ThingComponent>().VNum == vnum);
        }
    }

    public static class EntityExtensions
    {
        public static ThingComponent AsThing(this Entity self) => self.TryGetComponent(out ThingComponent component) ? component : null;

        public static AreaComponent AsArea(this Entity self) => self.TryGetComponent(out AreaComponent component) ? component : null;

        public static NetworkComponent AsNetworkable(this Entity self) => self.TryGetComponent(out NetworkComponent component) ? component : null;
    }
}