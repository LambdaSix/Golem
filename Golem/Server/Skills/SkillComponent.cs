using System.Collections.Generic;
using System.IO;
using Capsicum.Serialization;
using Golem.Game;

namespace Golem.Server.Skills
{
    public class SkillsComponent : ISerializableComponent
    {
        /// <summary>
        /// Mapping between SkillId and SkillLevel
        /// </summary>
        public Dictionary<Skill, float> Skills { get; set; }

        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                var itemCount = Skills.Count;
                bw.Write(itemCount);

                foreach (var pair in Skills)
                {
                    bw.Write((int)pair.Key);
                    bw.Write(pair.Value);
                }

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                var itemCount = br.ReadInt32();

                var dict = new Dictionary<Skill, float>(itemCount);

                for (int i = 0; i < itemCount; i++)
                {
                    var key = br.ReadInt32();
                    var value = br.ReadSingle();
                    dict.Add((Skill)key, value);
                }
            }
        }
    }
}