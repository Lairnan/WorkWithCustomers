using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace INCOMSYSTEM.BehaviorsFiles
{
    public class CountDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32((byte)value).ConvertDay();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}