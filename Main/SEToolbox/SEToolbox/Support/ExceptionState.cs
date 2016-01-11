namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using SEToolbox.Converters;

    public class ToolboxException : ArgumentException
    {
        private readonly string _friendlyMessage;

        public ToolboxException(ExceptionState state, params string[] arguments)
        {
            var converter = new EnumToResouceConverter();
            Arguments = arguments;
            _friendlyMessage = string.Format((string)converter.Convert(state, typeof(string), null, CultureInfo.CurrentUICulture), Arguments);
        }

        public override string Message
        {
            get
            {
                return _friendlyMessage;
            }
        }

        public string[] Arguments { get; private set; }
    }
}
