using Golem.Server.Enumerations;
using Golem.Server.Interfaces;

namespace Golem.Server.Account
{
    public class Account : IAccount
    {
        /// <inheritdoc />
        public string Username { get; }

        /// <inheritdoc />
        public string Email { get; }

        /// <inheritdoc />
        public AccessLevel AccessLevel { get; set; }

        /// <inheritdoc />
        public IMobile this[int index]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void SetPassword(string password)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void CheckPassword(string password)
        {
            throw new System.NotImplementedException();
        }
    }
}