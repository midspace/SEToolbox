namespace SEToolbox.Support
{
    using SEToolbox.Converters;
    using System;
    using System.Globalization;

    public enum ExceptionState
    {
        OK,
        NoRegistry,
        NoDirectory,
        NoApplication
    };

    public class ToolboxException : ArgumentException
    {
        private string friendlyMessage;

        public ToolboxException(ExceptionState state)
        {
            EnumToResouceConverter converter = new EnumToResouceConverter();
            this.friendlyMessage = (string)converter.Convert(state, typeof(string), null, CultureInfo.CurrentUICulture);
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
