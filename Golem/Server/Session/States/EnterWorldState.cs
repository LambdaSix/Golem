using Capsicum;
using Golem.Game.Mobiles;

namespace Golem.Server.Session.States
{
    public class EnterWorldState : SessionState
    {
        private readonly Entity _player;

        public EnterWorldState(Entity player)
        {
            _player = player;
        }

        /// <inheritdoc />
        public override void OnStateInitialize()
        {
            Session.Player = _player;

            if (Session.Player != null)
                Session.Player.GetComponent<NetworkStateComponent>().LoggedIn = true;

            // Session implements IOutputTextWriter, just looks weird :)
            _player.GetComponent<NetworkStateComponent>().NetSession = Session;

            //var room = GolemServer.Current.Database.Get<Room>(_player.Location);

            base.OnStateInitialize();
        }
    }
}