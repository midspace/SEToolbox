namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;

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
                    parseValue = Int32.Parse((String)value, null);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((parseValue < Min) || (parseValue > Max))
            {
                return new ValidationResult(false,
                  "Please enter a value in the range: " + Min + " - " + Max + ".");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
