namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using SEToolbox.Converters;

    public class ToolboxException : ArgumentException
    {
        private ExceptionState _state;
        private readonly string _friendlyMessage;

        public ToolboxException(ExceptionState state, params string[] arguments)
        {
            var converter = new EnumToResouceConverter();
            this._state = state;
            this.Arguments = arguments;
            this._friendlyMessage = string.Format((string)converter.Convert(state, typeof(string), null, CultureInfo.CurrentUICulture), this.Arguments);
        }

        public override string Message
        {
            get
            {
                return this._friendlyMessage;
            }
        }

        public string[] Arguments { get; private set; }
    }
}
