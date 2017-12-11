using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Golem.Game.Mobiles;

namespace Golem.Server.Session
{
    public class SessionMonitor : ISessionMonitor
    {
        private List<ISession> Sessions = new List<ISession>();

        /// <inheritdoc />
        public void RegisterSession(ISession session)
        {
            if (Sessions.Contains(session))
                throw new ArgumentException("Session already registered", nameof(session));

            session.SessionEnded += OnSessionEnded;
            Sessions.Add(session);
        }

        public ISession FindSessionForPlayer(IPlayer player) => Sessions.Single(s => s.Player == player);

        private void OnSessionEnded(object sender, EventArgs e)
        {
            if (!(sender is ISession))
                return;

            Sessions.Remove((ISession) sender);
        }

        /// <inheritdoc />
        public IEnumerator<ISession> GetEnumerator() => Sessions.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Sessions).GetEnumerator();
    }
}