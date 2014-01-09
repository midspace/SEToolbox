namespace SEToolbox.Support
{
    using SEToolbox.Converters;
    using System;
    using System.Globalization;

    public class ToolboxException : ArgumentException
    {
        private ExceptionState state;
        private string friendlyMessage;
        private string[] arguments;

        public ToolboxException(ExceptionState state, params string[] arguments)
        {
            EnumToResouceConverter converter = new EnumToResouceConverter();
            this.state = state;
            this.arguments = arguments;
            this.friendlyMessage = string.Format((string)converter.Convert(state, typeof(string), null, CultureInfo.CurrentUICulture), this.arguments);
        }

        public override string Message
        {
            get
            {
                return this.friendlyMessage;
            }
        }
    }
}
