using Capsicum;
using Golem.Server.Skills;

namespace Golem.Game
{
    public class GeneralAction
    {

    }

    public class ButcherAction : GeneralAction
    {
        public Entity Actor { get; }
        public Entity Corpse { get; }

        public ButcherAction(Entity actor, Entity corpse)
        {
            Actor = actor;
            Corpse = corpse;
        }

        public void Perform()
        {
            // The skill value lives in the Actor's SkillsComponent, but the concept of the 'butchery' skill lives in reference data
            Actor.GetComponent<SkillsComponent>()?.Skills.TryGetValue(Skill.Butchery, out float butcherySkill);

            // Butcher the Corpse, beyond the example here
            
            // The Actor used this skill so maybe they can roll for a skill improvement
            SkillSystem.ImproveSkill(Actor, Skill.Butchery);
        }

        public int Cooldown => (int)(6 * Actor.GetComponent<SkillsComponent>()?.Skills[Skill.Butchery] ?? 0.0f) / 100;
    }

    public enum Skill
    {
        Butchery = 1
    }

    public static class SkillSystem
    {
        public static void ImproveSkill(Entity actor, Skill skill)
        {

        }
    }
}