using Capsicum;
using Capsicum.Interfaces;
using Golem.Server.Mobiles;

namespace Golem.Game.Mobiles
{
    public class Dragon : IEntityAspect
    {
        public void Setup(Entity entity)
        {
            entity.AddComponent(new HealthComponent()
            {
                MaxHitpoints = 100,
                CurrentHitpoints = 100,
            });
            entity.AddComponent(new StatisticsComponent()
            {
                Strength = 18,
                Dexterity = 14,
                Constitution = 16,
                Intelligence = 19,
                Wisdom = 20,
                Luck = 16,
                Charisma = 10
            });
            entity.AddComponent(new DescribableComponent()
            {
                Name = "A Dragon",
                Description = "A large dragon",
                Keywords = new[] {"dragon"}
            });
        }
    }
}