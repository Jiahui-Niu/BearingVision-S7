using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp1.ViewModel
{
    public class IndexPlusOneConverter : IValueConverter
    {
        public static readonly IndexPlusOneConverter Instance = new IndexPlusOneConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i) return (i + 1).ToString();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolVisConverter : IValueConverter
    {
        public static readonly InverseBoolVisConverter Instance = new InverseBoolVisConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool bv && bv;
            return b ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
