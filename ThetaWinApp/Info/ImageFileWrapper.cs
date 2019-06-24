using System;
using System.Windows.Media.Imaging;
using ThetaNetCore.Common;

namespace ThetaWinApp.Info
{
	public class ImageFileWrapper : BindableBase
	{
		/*
		public StorageFile File { get; set; }

		private BitmapSource _thumbImage = null;
		public BitmapSource ThumbImage
		{
			get { return _thumbImage; }
			set { SetProperty<BitmapSource>(ref _thumbImage, value); }
		}

		public String FileName
		{
			get { return File.Name; }
		}

		DateTime? _dt = null;
		public DateTime TimeTaken
		{
			get
			{
				if (_dt == null)
				{
					var props = File.Properties.GetImagePropertiesAsync().AsTask().Result;
					_dt = props.DateTaken.DateTime;
				}
				return _dt.Value;
			}
		}

		public String SimpleDate
		{
			get
			{
				var props = File.Properties.GetImagePropertiesAsync().AsTask().Result;
				return new DateTimeFormatter("longdate").Format(props.DateTaken);
			}
		}

		public String DateAndTime
		{
			get
			{
				var props = File.Properties.GetImagePropertiesAsync().AsTask().Result;
				var dt = props.DateTaken;
				return dt.ToLocalTime().ToString();
			}
		}

		String _dateTaken = "";
		public String CompareDate
		{
			get
			{
				if (String.IsNullOrEmpty(_dateTaken))
				{
					var props = File.Properties.GetImagePropertiesAsync().AsTask().Result;
					_dateTaken = props.DateTaken.ToString("yyyy-MM-dd");
				}
				return _dateTaken;

			}
		}*/
	}
}
