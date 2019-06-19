using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThetaWinApp.Converters
{
	class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool trueIsVisible = true;
			if (parameter != null )
				bool.TryParse( parameter.ToString(), out trueIsVisible);

			if ((bool)value == trueIsVisible)
				return Visibility.Visible;
			else
				return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
