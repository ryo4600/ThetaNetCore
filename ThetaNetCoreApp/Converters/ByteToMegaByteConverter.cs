using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThetaNetCoreApp.Converters
{
	class ByteToMegaByteConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is long)
			{
				return Math.Round((long)value / 1024.0 / 1024.0, 2);
			}
			else
			{
				// Error case... show the control any way
				return value;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
