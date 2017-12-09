namespace Golem.Server.Session
{
    public abstract class SessionState {
        protected internal ISession Session { get; internal set; }

        public abstract void OnInput(string input);

        public abstract void OnStateInitialize();

        public abstract void OnStateEnter();
        public abstract void OnStateLeave();

        public abstract void OnStateShutdown();
    }
}