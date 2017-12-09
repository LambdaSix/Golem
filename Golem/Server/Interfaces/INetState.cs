using Capsicum;

namespace Golem.Server.Interfaces
{
    public interface INetState
    {
        IAccount Account { get; }
        Entity Entity { get; }

        /// <summary>
        /// Binds this INetState to a Client
        /// </summary>
        void Start();

        /// <summary>
        /// Sends a message to the user's client
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);


        /// <summary>
        /// Checks if this client has exceeded the idle timeout, so we can remove the netstate object
        /// </summary>
        /// <param name="curTicks"></param>
        void CheckAlive(long curTicks);
    }
}