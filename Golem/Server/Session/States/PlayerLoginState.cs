using System.Collections;
using Golem.Game.Mobiles;
using Golem.Server.Account;

namespace Golem.Server.Session.States
{
    public class PlayerLoginState : SessionState
    {
        private enum State
        {
            RequestUsername,
            RequestPassword
        }

        private State _currentState;
        private string _playerName;
        private string _password;
        private IPlayer _player;
        private const int MaxPasswordAttempts = 3;
        private int _currentPasswordAttempt;

        private void ChangeState(State state)
        {
            _currentState = state;

            switch (state)
            {
                case State.RequestUsername:
                    Session.Write("Username (type any name to create a new character): ");
                    break;
                case State.RequestPassword:
                    Session.Write("Password: ");
                    Session.SetEcho(false);
                    break;
            }
        }

        private IPlayer GetPlayer(string playerName)
        {
            return GolemServer.Current.Database.Get<Player>(Player.NameToKey(playerName));
        }

        public override void OnStateEnter()
        {
            if (_player != null)
            {
                Session.PushState(new EnterWorldState(_player));
            }

            ChangeState(State.RequestUsername);
            base.OnStateEnter();
        }

        public override void OnInput(string input)
        {
            switch (_currentState)
            {
                case State.RequestUsername:
                    _playerName = input;
                    _player = GetPlayer(_playerName);

                    if (_player == null)
                    {
                        if (!Accounts.ValidateUsername(input))
                        {
                            Session.WriteLine("Invalid username");
                            ChangeState(State.RequestUsername);
                            break;
                        }

                        Session.PushState(new CreateNewPlayerState(_playerName));
                        break;
                    }

                    ChangeState(State.RequestPassword);
                    break;

                case State.RequestPassword:
                    _password = input;
                    Session.SetEcho(true);

                    if (!_player.CheckPassword(_password))
                    {
                        Session.WriteLine("Invalid password");
                        ChangeState(State.RequestPassword);
                        _currentPasswordAttempt++;
                        if (_currentPasswordAttempt >= MaxPasswordAttempts)
                        {
                            Session.WriteLine("Too many failed attempts, disconnecting..");
                            Session.End();
                        }
                        break;
                    }

                    var existingSession = GolemServer.Current.SessionMonitor.FindSessionForPlayer(_player);

                    if (existingSession != null)
                    {
                        Session.WriteLine("Taking control of another active session...");
                        Session.HandConnectionTo(existingSession);
                    }

                    Session.Player = _player;
                    _player.LoggedIn = true;
                    Session.PushState(new EnterWorldState(_player));
                    break;
            }

            base.OnInput(input);
        }

        /// <inheritdoc />
        public override void OnStateLeave()
        {
            if (_player != null)
                _player.LoggedIn = false;

            base.OnStateLeave();
        }
    }
}