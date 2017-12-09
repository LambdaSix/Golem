using System;
using System.IO;
using System.Text;

namespace Golem.Server.UserInterface
{
    public class SimpleWordLexer
    {
        private StringReader _reader;
        private TextToken _next;

        public SimpleWordLexer(string text)
        {
            _reader = new StringReader(text);
        }

        private bool isWhitespace(int c) => c == ' ' || c == '\t' || c == '\f';
        private bool Separator(int c) => c == '.' || c == ',' || c == ':' || c == ';' || c == '!' || c == '?';
        private bool isNewline(int c) => c == '\n' || c == '\r';


        private void SkipWhitespace()
        {
            int c = _reader.Peek();
            while (isWhitespace(c))
            {
                _reader.Read();
                c = _reader.Peek();
            }
        }

        private TextToken ParseNewLine()
        {
            int c = _reader.Read();

            if (c == '\r')
            {
                c = _reader.Peek();
                if (c == '\n')
                {
                    _reader.Read();
                }

                return new TextToken() {Type = TextTokenType.Newline};
            }

            if (c == '\n')
                return new TextToken() {Type = TextTokenType.Newline};

            throw new InvalidOperationException("Unrecognised characters for newline");
        }

        private TextToken ParseWord()
        {
            var wordBuilder = new StringBuilder();

            int c = _reader.Peek();
            while (!isWhitespace(c) && !Separator(c) && c != '\r' && c != '\n' && c != -1)
            {
                wordBuilder.Append((char) c);
                _reader.Read();
                c = _reader.Peek();
            }

            return new TextToken()
            {
                Type = TextTokenType.Word,
                Text = wordBuilder.ToString()
            };
        }

        public TextToken GetNext()
        {
            if (_next != null)
            {
                try
                {
                    return _next;
                }
                finally
                {
                    _next = null;
                }
            }

            SkipWhitespace();

            int c = _reader.Peek();

            if (c == -1)
            {
                return new TextToken() {Type = TextTokenType.Eof};
            }

            if (Separator(c))
            {
                _reader.Read();
                return new TextToken() {Type = TextTokenType.Separator, Text = ((char) c).ToString()};
            }

            if (isNewline(c))
                return ParseNewLine();

            return ParseWord();
        }

        public TextToken Peek()
        {
            if (_next != null)
                return _next;

            _next = GetNext();
            return _next;
        }
    }
}