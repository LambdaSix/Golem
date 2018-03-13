using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Capsicum.Serialization;
using Golem.Game.Mobiles;

namespace Golem.Server.Mobiles
{
    public class StatisticsComponent : ISerializableComponent
    {
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public int Luck { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(Strength);
                bw.Write(Dexterity);
                bw.Write(Constitution);
                bw.Write(Intelligence);
                bw.Write(Wisdom);
                bw.Write(Charisma);
                bw.Write(Luck);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                Strength = br.ReadInt32();
                Dexterity = br.ReadInt32();
                Constitution = br.ReadInt32();
                Intelligence = br.ReadInt32();
                Wisdom = br.ReadInt32();
                Charisma = br.ReadInt32();
                Luck = br.ReadInt32();
            }
        }
    }

    public class CombatantComponent : ISerializableComponent
    {
        public int Armour { get; set; }
        public int DamageRoll { get; set; }
        public int HitRoll { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(Armour);
                bw.Write(DamageRoll);
                bw.Write(HitRoll);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                Armour = br.ReadInt32();
                DamageRoll = br.ReadInt32();
                HitRoll = br.ReadInt32();
            }
        }
    }

    public class PostureComponent : ISerializableComponent
    {
        public CharacterPosture Posture { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write((int) Posture);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                Posture = (CharacterPosture) br.ReadInt32();
            }
        }
    }

    public class HealthComponent : ISerializableComponent
    {
        public int MaxHitpoints { get; set; }
        public int CurrentHitpoints { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(MaxHitpoints);
                bw.Write(CurrentHitpoints);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                MaxHitpoints = br.ReadInt32();
                CurrentHitpoints = br.ReadInt32();
            }
        }
    }

    public class RespawnableComponent : ISerializableComponent
    {
        public int MinDelay { get; set; }
        public int MaxDelay { get; set; }
        public int RespawnZone { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(MinDelay);
                bw.Write(MaxDelay);
                bw.Write(RespawnZone);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                MinDelay = br.ReadInt32();
                MaxDelay = br.ReadInt32();
                RespawnZone = br.ReadInt32();
            }
        }
    }

    public class LocationComponent : ISerializableComponent
    {
        public long RoomId { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(RoomId);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                RoomId = br.ReadInt64();
            }
        }
    }

    public class DescribableComponent : ISerializableComponent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Keywords { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(Name);
                bw.Write(Description);
                bw.Write(Keywords.Count());
                foreach (var keyword in Keywords)
                {
                    bw.Write(keyword);
                }

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                Name = br.ReadString();
                Description = br.ReadString();

                var keywordCount = br.ReadInt32();
                var keywordsList = new List<string>(keywordCount);

                for (int i = 0; i < keywordCount; i++)
                {
                    keywordsList[i] = br.ReadString();
                }

                Keywords = keywordsList;
            }
        }
    }
}