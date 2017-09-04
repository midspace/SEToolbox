namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using WPFLocalizeExtension.Extensions;

    public class I18NFormatterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return values;

            var bindingParams = (object[])values.Clone();

            // Remove the {DependancyProperty.UnsetValue} from unbound datasources.
            for (var i = 0; i < bindingParams.Length; i++)
            {
                if (bindingParams[i] != null && bindingParams[i] == DependencyProperty.UnsetValue)
                {
                    bindingParams[i] = null;
                }
            }

            var keyStr = (string)parameter;
            var ext = new LocExtension(keyStr);
            string localizedValue;
            return ext.ResolveLocalizedValue(out localizedValue) ? string.Format(localizedValue, bindingParams) : null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}
