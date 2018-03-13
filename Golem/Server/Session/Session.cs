using System;
using System.Collections.Generic;
using System.IO;
using Capsicum;
using Capsicum.Interfaces;
using Capsicum.Serialization;
using Golem.Game.Mobiles;
using Golem.Server.Network;
using Golem.Server.Text;

namespace Golem.Server.Session
{
    public class NetworkStateComponent : ISerializableComponent
    {
        public string AccountName { get; set; }
        public bool LoggedIn { get; set; }

        // Transient by connection
        public int SessionId { get; set; }

        // Transient by connection
        public ISession NetSession { get; set; }


        /// <inheritdoc />
        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(AccountName);
                bw.Write(LoggedIn);

                return (bw.BaseStream as MemoryStream)?.ToArray();
            }
        }

        /// <inheritdoc />
        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                AccountName = br.ReadString();
                LoggedIn = br.ReadBoolean();
            }
        }
    }

    public class Session : ISession
    {
        private Stack<SessionState> SessionStates { get; } = new Stack<SessionState>();
        private IConnection Connection { get; set; }

        public ITextTransformer OutputTransformer { get; set; }
        public Entity Player { get; set; }

        public event EventHandler SessionEnded;

        public SessionState CurrentState => SessionStates.Count == 0 ? null : SessionStates.Peek();

        public Session(IConnection connection)
        {
            Connection = connection;
            SetHandlers(connection);
        }

        public void HandConnectionTo(ISession session)
        {
            session.WriteLine("This player has been logged in from else where");
            session.ChangeConnection(RelinquishConnectionOwnership());
            End();
        }

        public void ChangeConnection(IConnection newConnection)
        {
            if (Connection != null)
            {
                UnsetHandlers(Connection);
                Connection.Close();
            }

            if (newConnection != null)
            {
                Connection = newConnection;
                SetHandlers(Connection);
            }
        }

        public void PushState(SessionState state)
        {
            if (state == null)
                throw new ArgumentException("state is null", nameof(state));

            CurrentState?.OnStateLeave();

            state.Session = this;
            SessionStates.Push(state);
            state.OnStateInitialize();
            state.OnStateEnter();
        }

        public SessionState PopState()
        {
            if (CurrentState == null)
                return null;

            var poppedState = SessionStates.Pop();
            poppedState.OnStateLeave();
            poppedState.OnStateShutdown();

            CurrentState?.OnStateEnter();

            return poppedState;
        }

        public void Write(string value)
        {
            if (Connection == null)
                return;

            if (OutputTransformer != null)
                value = OutputTransformer.Transform(value);

            Connection.Write(value);
        }

        public void Write(object value) => Write(value.ToString());
        public void Write(string format, params object[] args) => Write(String.Format(format, args));

        public void WriteLine(string value)
        {
            if (Connection == null)
                return;

            if (OutputTransformer != null)
                value = OutputTransformer.Transform(value);

            Connection.WriteLine(value);
        }

        public void WriteLine(object value) => WriteLine(value.ToString());
        public void WriteLine(string format, params object[] args) => WriteLine(String.Format(format, args));

        public void End()
        {
            if (Player != null)
                Player.GetComponent<NetworkStateComponent>().LoggedIn = false;

            foreach (var session in SessionStates)
            {
                session.OnStateShutdown();
            }

            Connection?.Close();
            OnSessionEnded();
        }

        /// <inheritdoc />
        public void SetEcho(bool echoState)
        {
            Connection.SetEcho(echoState);
        }

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            Connection = null;
            End();
        }

        private void OnInputReceived(object sender, LineReceivedEventArgs e)
        {
            CurrentState?.OnInput(e.Data);
        }

        private void OnSessionEnded()
        {
            SessionEnded?.Invoke(this, EventArgs.Empty);
        }

        private IConnection RelinquishConnectionOwnership()
        {
            if (Connection != null)
            {
                UnsetHandlers(Connection);

                var conn = Connection;
                Connection = null;
                return conn;
            }

            return null;
        }

        private void UnsetHandlers(IConnection connection)
        {
            connection.Closed -= OnConnectionClosed;
            connection.LineReceived -= OnInputReceived;
        }
        private void SetHandlers(IConnection connection)
        {
            connection.LineReceived += OnInputReceived;
            connection.Closed += OnConnectionClosed;
        }
    }
}