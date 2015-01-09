#if !WINDOWS_FORM_APP
using System;
#if WINDOWS_PRESENTATION_APP
using System.Globalization;
using System.Windows;
using System.Windows.Data;
#endif
#if WINDOWS_APP||WINDOWS_PHONE_APP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace SoftwareKobo.UI.Converters
{
    public class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
#if WINDOWS_PRESENTATION_APP
            CultureInfo culture
#endif
#if WINDOWS_APP || WINDOWS_PHONE_APP
            string language
#endif
            )
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if WINDOWS_PRESENTATION_APP
            CultureInfo culture
#endif
#if WINDOWS_APP || WINDOWS_PHONE_APP
            string language
#endif
            )
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is bool?)
            {
                bool? tmp = (bool?)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }
            return (bValue) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
#endif