namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using SEToolbox.Converters;

    [Serializable]
    public class ToolboxException : ArgumentException
    {
        private readonly string _friendlyMessage;

        public ToolboxException(ExceptionState state, params object[] arguments)
        {
            var converter = new EnumToResouceConverter();
            Arguments = arguments;
            _friendlyMessage = string.Format((string)converter.Convert(state, typeof(string), null, CultureInfo.CurrentUICulture), Arguments);
        }

        public override string Message
        {
            get { return _friendlyMessage; }
        }

        public object[] Arguments { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
