using System;
using System.Collections;
using System.Collections.Generic;

namespace Golem.Server.World
{
    [Flags]
    public enum Directions : Int32
    {
        North = 0x00000001,
        South = 0x00000002,
        East = 0x00000004,
        West = 0x00000008,
        Up = 0x00000010,
        Down = 0x00000020,
        Out = 0x00000040,
        In = 0x00000080
    }

    [Flags]
    public enum RoomType : Int64
    {
        Inside = 0x00000001,
        Watery = 0x00000002,
        Airy = 0x00000004,
        Fiery = 0x00000008,
        Earthy = 0x00000010,
        Noisy = 0x00000020,
        Peaceful = 0x00000040,
        Dark = 0x00000080,
        ExtraMana = 0x00000100,
        ExtraHeal = 0x00000200,
        ManaDrain = 0x00000400,
        NoMagic = 0x00000800,
        Underground = 0x00001000,
        NoTracks = 0x00002000,
        NoBlood = 0x00004000,
        EasyMove = 0x00008000,
        Rough = 0x00010000,
        Forest = 0x00020000,
        Field = 0x00040000,
        Desert = 0x00080000,
        Swamp = 0x00100000,
        NoRecall = 0x00200000,
        NoSummon = 0x00400000,
        Underwater = 0x00800000,

        /// <summary>
        /// Can be mined/harvested.
        /// </summary>
        Minerals = 0x01000000,

        /// <summary>
        /// Inside of the room is lit
        /// </summary>
        Light = 0x02000000,

        /// <summary>
        /// Requires GEAR_CLIMB to move in
        /// </summary>
        Mountain = 0x04000000,

        /// <summary>
        /// Snow, uses a lot of moves per move
        /// </summary>
        Snow = 0x08000000,

        /// <summary>
        /// Otherworldly room
        /// </summary>
        Astral = 0x10000000,

        /// <summary>
        /// Can't map this room
        /// </summary>
        NoMap = 0x20000000,

        /// <summary>
        /// These rooms required special gear to move or survive in
        /// </summary>
        BadRoom = (Watery | Earthy | Underwater | Airy | Mountain | Astral),

        /// <summary>
        /// Flags indicating the room has a certain sector
        /// </summary>
        RoomSector = (Inside | Watery | Airy | Fiery | Earthy | Underground | EasyMove | Underwater | Rough | Forest |
                      Field | Desert | Swamp | Mountain | Snow | Astral),
    }

    public struct RoomData
    {
        public string Name;
        public string App;
        public RoomType FlagVal;
        public string Help;

        public RoomData(string name, string app, RoomType flagVal, string help)
        {
            Name = name;
            App = app;
            FlagVal = flagVal;
            Help = help;
        }
    }

    public static class RoomFlags
    {
        public static RoomFlagData Flags = new RoomFlagData();
    }

    public class RoomFlagData : IEnumerable<RoomData>
    {
        public static List<RoomData> RoomFlags = new List<RoomData>
        {
            new RoomData("inside", " Inside", RoomType.Inside, "This room is inside."),
            new RoomData("watery", " Watery", RoomType.Watery, "This room is water - need Swim or Boat."),
            new RoomData("airy", " Airy", RoomType.Airy, "This room is airy. Need to Fly."),
            new RoomData("fiery", " Fiery", RoomType.Fiery, "This room is fiery - Burns you."),
            new RoomData("earthy", " Earthy", RoomType.Earthy, "This room is earthy - undeground, crushes."),
            new RoomData("noisy", " Noisy", RoomType.Noisy, "This room is noisy, communications are harder."),
            new RoomData("peaceful", " Peaceful", RoomType.Peaceful, "No fighting here!"),
            new RoomData("dark", " Dark", RoomType.Dark, "This room is dark."),
            new RoomData("extraheal", " ExtraHeal", RoomType.ExtraHeal, "This room heals faster."),
            new RoomData("extramana", " ExtraMana", RoomType.ExtraMana, "This room gives mana back faster."),
            new RoomData("manadrain", " ManaDrain", RoomType.ManaDrain, "This room takes mana away."),
            new RoomData("nomagic", " NoMagic", RoomType.NoMagic, "This room prevents magic from working."),
            new RoomData("underground", " Underground", RoomType.Underground, "This room is underground."),
            new RoomData("notracks", " NoTracks", RoomType.NoTracks, "No tracks are left behind in this room."),
            new RoomData("noblood", " NobLood", RoomType.NoBlood, "No blood is made in this room."),
            new RoomData("easymode", "EasyMove", RoomType.EasyMove, "This room is easier to move through, no shoes required."),
            new RoomData("rough", " Rough", RoomType.Rough, "This room is harder to move through."),
            new RoomData("forest", " Forest", RoomType.Forest, "Trees everywhere."),
            new RoomData("field", " Field", RoomType.Field, "A large field."),
            new RoomData("desert", " Desert", RoomType.Desert, "A large dry area."),
            new RoomData("swamp", " Swamp", RoomType.Swamp, "Hot, watery area."),
            new RoomData("norecall", " NoRecall", RoomType.NoRecall, "Cannot recall/leave with magic."),
            new RoomData("nosummon", " NoSummon", RoomType.NoSummon, "Cannot summon things here with magic."),
            new RoomData("underwater", " Underwater", RoomType.Underwater, "Need to be able to breath underwater."),
            new RoomData("minerals", " Minerals", RoomType.Minerals, "Can be harvested, will deplete."),
            new RoomData("light", " Light", RoomType.Light, "There is light in here."),
            new RoomData("mountain", " Mountain", RoomType.Mountain, "Need boots to climb here."),
            new RoomData("snow", " Snow", RoomType.Snow, "Cold, tracks fade quickly."),
            new RoomData("astral", " Astral", RoomType.Astral, "Powerfull beings reside here."),
            new RoomData("nomap", " NoMap", RoomType.NoMap, "This room cannot be mapped."),
            new RoomData("none", "none", 0, "nothing")
        };

        /// <inheritdoc />
        public IEnumerator<RoomData> GetEnumerator() => RoomFlags.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => RoomFlags.GetEnumerator();
    }

    public class RoomComponent : IComponent
    {
        public int Id { get; set; }
        public RoomType Type { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Directions ExitDirections { get; set; }

        // TODO: Foraging, Botanising, & Harvesting
        //public ForageInformation Forageable { get; set; }
        //public BotaniseInformation Botanisable { get; set; }
        //public HarvestInformation Harvestable { get; set; }

        public bool ContainsCamp { get; set; }

        /// <summary>
        /// All things 'in' this Room.
        /// </summary>
        public IEnumerable<Entity> Contents { get; set; }
    }
}