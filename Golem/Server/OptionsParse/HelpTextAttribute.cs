using System;

namespace Golem.Server.OptionsParse {
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class HelpTextAttribute : Attribute {
        private readonly string _defaultHelpText;
        public string DefaultHelpText { get { return _defaultHelpText; } }

        public int LocalisedId { get; set; }

        public HelpTextAttribute(string defaultHelpText) {
            _defaultHelpText = defaultHelpText;
        }
    }
}