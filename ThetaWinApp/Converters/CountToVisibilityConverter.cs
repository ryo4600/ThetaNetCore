using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThetaWinApp.Converters
{
	class CountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int iVal;
			if (Int32.TryParse(value.ToString(), out iVal))
			{
				return iVal > 0 ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				// Error case... show the control any way
				return Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
