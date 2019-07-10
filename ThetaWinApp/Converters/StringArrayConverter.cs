using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ThetaWinApp.Converters
{
	class StringArrayConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var strArray = value as String[];
			if (strArray == null)
				return "";

			var delimeter = ", ";
			if (parameter != null)
				delimeter = parameter.ToString();

			var builder = new StringBuilder();
			foreach(var str in strArray)
			{
				if (builder.Length > 0)
					builder.Append(delimeter);
				builder.Append(str);
			}
			return builder.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
