namespace Golem.Server.UserInterface
{
    public enum TextTokenType
    {
        Eof,
        Newline,
        Word,
        Separator
    }

    public class TextToken
    {
        public TextTokenType Type { get; set; }
        public string Text { get; set; }
    }
}