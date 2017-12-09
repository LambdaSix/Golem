using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Text;
using Golem.Server.Extensions;
using Golem.Server.Session;

namespace Golem.Server.UserInterface
{
    public class TextEditor : SessionState
    {
        private List<string> _lines = new List<string>();

        /// <summary>
        /// Descript of the thing the user is editing
        /// </summary>
        public string Description { get; set; }

        public string Result => String.Join("\n", _lines);

        public bool Success { get; private set; }

        public event EventHandler<string> OnSuccess;

        public TextEditor() { }

        public TextEditor(string initialText)
        {
            // TODO: Write a better string splitter (part of StringLexer?)
            _lines = new List<string>(initialText.Split(new[] {"\n"}, StringSplitOptions.None));
        }

        private void PrintHelp()
        {
            // TODO: Globalise this string
            Session.WriteLine("Enter text line by line to append it to the text buffer.");
            Session.WriteLine("There are also a number of single letter commands you can enter.");
            Session.WriteLine("h            - Displays this help message.");
            Session.WriteLine("x            - Exit the text editor without saving the changes.");
            Session.WriteLine("s            - Save and exit the text editor.");
            Session.WriteLine("c            - Clears all text in the buffer.");
            Session.WriteLine("l            - Lists the current text.");
            Session.WriteLine("f            - Formats your text.");
            Session.WriteLine("r l <text>   - Replaces line number l with text <text>.");
            Session.WriteLine("d l          - Deletes line number l.");
            Session.WriteLine("");
            Session.WriteLine("When auto formating is invoked (f), single line breaks are");
            Session.WriteLine(" ignored while double line breaks are treated as end of paragraph");
            Session.WriteLine("markers which cause there to be an empty line in the final output.");
        }

        private void PrintCurrentText()
        {
            Session.WriteLine("Current text:");
            if (_lines.Count == 0)
                Session.WriteLine("No text entered");

            for (int i = 0; i < _lines.Count; i++)
            {
                Session.WriteLine($"{i:3}: {_lines[i]}");
            }
        }

        private void FormatText()
        {
            const int maxLineLength = 75;
            var lines = new List<string>();
            var lexer = new SimpleWordLexer(Result);
            var currentLine = new StringBuilder();

            for (var token = lexer.GetNext(); token.Type != TextTokenType.Eof; token = lexer.GetNext())
            {
                if (token.Type == TextTokenType.Newline)
                {
                    var next = lexer.Peek();

                    if (next.Type == TextTokenType.Newline)
                    {
                        lines.Add(currentLine.ToString());
                        lines.Add(string.Empty);
                        currentLine.Clear();
                        token = lexer.GetNext();
                    }
                }

                if (token.Type == TextTokenType.Word)
                {
                    var next = lexer.Peek();
                    var requiredExtraSpace = 0;

                    if (currentLine.Length > 0)
                        requiredExtraSpace = 1;

                    if (next.Type == TextTokenType.Separator)
                        requiredExtraSpace = 2;

                    if (currentLine.Length + token.Text.Length + requiredExtraSpace >= maxLineLength)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                    }

                    if (currentLine.Length > 0)
                        currentLine.Append(' ');

                    string textToAppend = token.Text;

                    while (textToAppend.Length > maxLineLength)
                    {
                        lines.Add(textToAppend.Substring(0, maxLineLength));
                        textToAppend = textToAppend.Substring(maxLineLength);
                        currentLine.Clear();
                    }

                    currentLine.Append(textToAppend);
                }

                if (token.Type == TextTokenType.Separator)
                    currentLine.Append(token.Text);
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            _lines = lines;
        }

        /// <inheritdoc />
        public override void OnStateInitialize()
        {
            throw new NotImplementedException();
        }

        public override void OnStateEnter()
        {
            Session.WriteLine("\f");
            Session.WriteLine(Description);
            Session.WriteLine("====================================================================");
            Session.WriteLine("Entering text editor.");
            Session.WriteLine("Type h for help, x to discard changes and exit the editor,");
            Session.WriteLine("s to save changes and exit the editor");
            Session.WriteLine("====================================================================");
            PrintCurrentText();
            Session.Write("\r\n> ");
        }

        /// <inheritdoc />
        public override void OnStateLeave()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void OnStateShutdown()
        {
            throw new NotImplementedException();
        }

        private void DoReplace(string commandArguments)
        {
            if (GetArgumentLineNumber(commandArguments, out var lineNumber, out var remaining)) return;

            _lines[lineNumber - 1] = remaining.TrimStart();
        }

        private void DoDelete(string commandArguments)
        {
            if (GetArgumentLineNumber(commandArguments, out var lineNumber, out var remaining)) return;

            _lines.RemoveAt(lineNumber - 1);
        }

        public override void OnInput(string input)
        {
            string remaining = input.ReadCommandLinePart(out var command);

            if (command.Length == 1)
            {
                switch (command.ToLower())
                {
                    case "h":
                        PrintHelp();
                        break;

                    case "x":
                        Success = false;
                        Session.PopState();
                        break;

                    case "s":
                        Success = true;
                        OnSuccess?.Invoke(this, Result);
                        Session.PopState();
                        break;

                    case "l":
                        PrintCurrentText();
                        break;

                    case "f":
                        FormatText();
                        break;

                    case "r":
                        DoReplace(remaining);
                        break;

                    case "d":
                        DoDelete(remaining);
                        break;

                    case "c":
                        _lines.Clear();
                        break;

                    default:
                        _lines.Add(input);
                        break;
                }
            }
            else
            {
                _lines.Add(input);
            }

            Session.Write("\n> ");
        }

        private bool GetArgumentLineNumber(string commandArguments, out int lineNumber, out string remaining)
        {
            remaining = commandArguments.ReadCommandLinePart(out var lineNumberStr);

            if (!Int32.TryParse(lineNumberStr, out lineNumber))
            {
                Session.WriteLine("First argument to replace was not the line number");
                return false;
            }

            if (lineNumber < 1 || lineNumber > _lines.Count)
            {
                Session.WriteLine($"Invalid line number, valid line numbers are 0-{_lines.Count}");
                return false;
            }
            return true;
        }

    }
}