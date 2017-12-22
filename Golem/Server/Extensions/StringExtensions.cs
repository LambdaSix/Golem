using System;
using System.Text;
using Golem.Game.Mobiles;

namespace Golem.Server.Extensions
{
    public static class StringHelpers
    {
        /// <summary>
        /// Select the appropriate 
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="player"></param>
        /// <param name="masculine">The masculine string to use</param>
        /// <param name="feminine">The feminine string to use</param>
        /// <param name="neuter"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        private static string SelectOnPronouns(IPlayer observer, IPlayer player, string masculine, string feminine, string neuter, string ob)
        {
            if (observer == player)
                return ob;

            // TODO: Custom pronouns and conjugations thereof.
            switch (player.Pronouns)
            {
                case PlayerPronouns.Masculine:
                    return masculine;
                case PlayerPronouns.Feminine:
                    return feminine;
                default:
                    return neuter;
            }
        }

        private static void ReplaceToken(StringBuilder result, char c, IPlayer observer, IPlayer subject, IPlayer target)
        {
            switch (c)
            {
                case 'o':
                    result.Append(SelectOnPronouns(observer, subject, "his", "her", "their", "your"));
                    break;
                case 'O':
                    result.Append(SelectOnPronouns(observer, target, "his", "her", "their", "your"));
                    break;
                case 'p':
                    result.Append(SelectOnPronouns(observer, subject, "him", "her", "they", "you"));
                    break;
                case 'P':
                    result.Append(SelectOnPronouns(observer, target, "him", "her", "they", "you"));
                    break;
                case 'n':
                    result.Append(SelectOnPronouns(observer, subject, "he", "she", "them", "you"));
                    break;
                case 'N':
                    result.Append(SelectOnPronouns(observer, target, "he", "she", "them", "you"));
                    break;
                case 'd':
                    result.Append(observer.GetOtherPlayerDescription(subject));
                    break;
                case 'D':
                    result.Append(observer.GetOtherPlayerDescription(target));
                    break;
                case 'y':
                    result.Append(subject == observer ? "you" : observer.GetOtherPlayerDescription(subject));
                    break;
                case 'Y':
                    result.Append(target == observer ? "you" : observer.GetOtherPlayerDescription(target));
                    break;

            }
        }

        /// <summary>
        /// his, her, their, your        %o
        /// him, her, them, you          %p
        /// he, she, they, you           %n
        /// description / name           %d
        ///
        /// lower case means subject, upper case means target
        /// "%d opens %o bag and hands %D a melon"
        /// </summary>
        /// <param name="format"></param>
        /// <param name="observer"></param>
        /// <param name="subject"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string BuildString(string format, IPlayer observer, IPlayer subject, IPlayer target)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == '%')
                {
                    i++;
                    ReplaceToken(result, format[i], observer, subject, target);
                    continue;
                }

                result.Append(format[i]);
            }

            return result.ToString();
        }

        public static string Capitalise(string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        public static string SplitLine(string value, int length = 80)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            if (!String.IsNullOrEmpty(value) && value.Length < 80)
                return value;

            var line = value.Substring(0, 80);
            line = line.Substring(0, line.LastIndexOf(' ')).Trim();
            value = value.Remove(0, line.Length).Trim();

            return line + "\n" + SplitLine(value);
        }

        public static string[] GetKeywords(string value)
        {
            return value.Split(new char[] {' ', '!', '?', ',', '.', ';', ':'}, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public static class StringExtensions
    {
        public static string ReadCommandLinePart(this string input, out string part)
        {
            // TODO: FSM yielder

            bool startFound = false;
            bool isReading = false;
            char sep = ' ';
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (!startFound)
                {
                    if (char.IsWhiteSpace(input[i]))
                        continue;
                    else
                    {
                        startFound = true;
                        i--;
                        continue;
                    }
                }

                if (!isReading)
                {
                    isReading = true;

                    if (input[i] == '"' || input[i] == '\'')
                    {
                        sep = input[i];
                        continue;
                    }

                    i--;
                    continue;
                }

                if (input[i] != sep)
                {
                    result.Append(input[i]);
                }
                else
                {
                    i++;
                    part = result.ToString();
                    if (i == input.Length)
                        return string.Empty;

                    return input.Substring(i);
                }
            }

            part = result.ToString();
            return string.Empty;
        }
    }
}