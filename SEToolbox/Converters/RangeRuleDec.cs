namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using Res = SEToolbox.Properties.Resources;

    public class RangeRuleDec : ValidationRule
    {
        public decimal Min { get; set; }

        public decimal Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal parseValue = 0;

            try
            {
                if (((string)value).Length > 0)
                    parseValue = decimal.Parse((string)value, null);
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
