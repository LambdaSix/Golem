using System;
using System.Collections.Generic;
using Golem.Common.Interfaces;

namespace Golem.Common.Events
{
    public class EventSink
    {
        public EventHandler<CharacterCreatedEventArgs> CharacterCreated { get; set; }
    }

    public class CharacterCreatedEventArgs : EventArgs
    {
        public CharacterCreatedEventArgs(INetState state, IAccount account, IMobile mobile, string name, string gender,
            int strength, int dexterity, int intelligence, IEnumerable<ISkillInfo> skills,
            IProfessionInfo profession, IRaceInfo race)
        {
            State = state;
            Account = account;
            Mobile = mobile;
            Name = name;
            Gender = gender;
            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
            Skills = skills;
            Profession = profession;
            Race = race;
        }

        public INetState State { get; }
        public IAccount Account { get; }
        public IMobile Mobile { get; set; }
        public string Name { get; }
        public string Gender { get; }
        public int Strength { get; }
        public int Dexterity { get; }
        public int Intelligence { get; }
        public IEnumerable<ISkillInfo> Skills { get; }
        public IProfessionInfo Profession { get; }
        public IRaceInfo Race { get; }
    }
}