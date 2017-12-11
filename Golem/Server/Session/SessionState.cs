namespace Golem.Server.Session
{
    public class SessionState {
        protected internal ISession Session { get; internal set; }

        public virtual void OnInput(string input)
        {
        }

        public virtual void OnStateInitialize()
        {
        }

        public virtual void OnStateEnter() { }
        public virtual void OnStateLeave()
        {
        }

        public virtual void OnStateShutdown()
        {             
        }
    }
}