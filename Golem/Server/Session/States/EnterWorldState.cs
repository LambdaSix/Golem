using Golem.Game.Mobiles;

namespace Golem.Server.Session.States
{
    public class EnterWorldState : SessionState
    {
        private readonly IPlayer _player;

        public EnterWorldState(IPlayer player)
        {
            _player = player;
        }

        /// <inheritdoc />
        public override void OnStateInitialize()
        {
            Session.Player = _player;

            if (Session.Player != null)
                Session.Player.LoggedIn = true;

            // Session implements IOutputTextWriter, just looks weird :)
            _player.SetOutputWriter(Session);

            var room = GolemServer.Current.Database.Get<Room>(_player.Location);

            base.OnStateInitialize();
        }
    }
}