using System;
using System.Windows.Data;
using System.Windows.Media;

namespace TodoAppNet.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Brushes.Gray;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(value.ToString());
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}