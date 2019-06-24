using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using ThetaWinApp.Info;

namespace ThetaToGo.Converter
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class FileGroupHeaderConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var wrappers = value as ObservableCollection<ImageFileWrapper>;
			if (wrappers == null)
				return "invalid";

			if (wrappers.Count == 0)
				return "empty";

			return wrappers[0].SimpleDate;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
