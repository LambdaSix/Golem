using System;
using System.IO;
using Golem.Game.Mobiles;
using Golem.Server.Network;
using Golem.Server.Text;

namespace Golem.Server.Session
{
    public interface ISession : IOutputTextWriter
    {
        event EventHandler SessionEnded;

        IPlayer Player { get; set; }

        void HandConnectionTo(ISession session);
        void ChangeConnection(IConnection connection);

        void PushState(SessionState state);
        SessionState PopState();

        void End();

        /// <summary>
        /// Change local client echo on/off
        /// </summary>
        /// <param name="echoState"></param>
        void SetEcho(bool echoState);
    }
}