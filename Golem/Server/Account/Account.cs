using System.Text.RegularExpressions;

namespace Golem.Server.Account
{
    public class Accounts
    {
        public static bool ValidateUsername(string input) => Regex.Match(input, @"^[a-zA-Z]{3,20}").Success;
    }
}