using Golem.Common.Enumerations;

namespace Golem.Common.Interfaces
{
    public interface IAccount
    {
        string Username { get; }
        string Email { get; }
        AccessLevel AccessLevel { get; set; }

        IMobile this[int index] { get; set; }

        void Delete();
        void SetPassword(string password);
        void CheckPassword(string password);
    }
}