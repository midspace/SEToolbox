namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using Res = SEToolbox.Properties.Resources;

    public class RangeRuleI : ValidationRule
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int parseValue = 0;

            try
            {
                if (((string)value).Length > 0)
                    parseValue = Int32.Parse((string)value, null);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, string.Format(Res.ValidationInvalidCharacters, Res.ValidationInvalidCharacters, e.Message));
            }

            if ((parseValue < Min) || (parseValue > Max))
            {
                return new ValidationResult(false, string.Format("{0} {1} - {2}.", Res.ValidationInvalidRange, Min, Max));
            }

            return new ValidationResult(true, null);
        }
    }
}
