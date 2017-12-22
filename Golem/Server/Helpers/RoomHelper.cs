using Golem.Game.Mobiles;
using Golem.Server.World;

namespace Golem.Server.Helpers
{
    public class RoomHelper
    {
        public static Room GetPlayerRoom(IPlayer player) => GetPlayerRoom(player.Location);
        public static Room GetPlayerRoom(string key) => GetRoom(key);

        public static Room GetRoom(string key) => GolemServer.Current.Database.Get<Room>(key);

        public static void SaveRoom(Room room) => GolemServer.Current.Database.Save(room);

        public static Area GetArea(string key) => GolemServer.Current.Database.Get<Area>(key);

        public static string GetDefaultRoomDescription =>
            "This exitless room has no description, you are surrouned by void";

        public static string GenerateKey(string key)
        {
            int index = 2;
            while (GolemServer.Current.Database.Get<Room>(key.ToLower() + index) != null)
            {
                index++;
            }
            return key + index;
        }
    }
}