using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using ThetaNetCore.Common;

namespace ThetaWinApp.Info
{
	public class ImageFileWrapper : BindableBase
	{

		private FileInfo _file;
		public FileInfo File { get => _file; set => _file = value;  }

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

		private Image _img;

		DateTime? _dt = null;
		public DateTime DateTaken
		{
			get
			{
				if (_dt == null)
				{
					using (var img = Image.FromFile(_file.FullName))
					{
						foreach (var anItem in img.PropertyItems)
						{
							// Extract date the picture is taken.
							if (anItem.Id == 0x9003 && anItem.Type == 2)
							{
								string val = System.Text.Encoding.ASCII.GetString(anItem.Value);
								val = val.Trim(new char[] { '\0' });
								_dt = DateTime.ParseExact(val, "yyyy:MM:dd HH:mm:ss", null);
							}
						}
					}
				}
				return _dt.Value;
			}
		}

		public String DateString
		{
			get
			{
				return DateTaken.ToShortDateString();
			}
		}

		public String TimeString
		{
			get
			{
				return DateTaken.ToShortTimeString();
			}
		}

		//public String DateAndTime
		//{
		//	get
		//	{
		//		var props = File.Properties.GetImagePropertiesAsync().AsTask().Result;
		//		var dt = props.DateTaken;
		//		return dt.ToLocalTime().ToString();
		//	}
		//}

		//String _dateTaken = "";
		//public String CompareDate
		//{
		//	get
		//	{
		//		if (String.IsNullOrEmpty(_dateTaken))
		//		{
		//			var props = File.Properties.GetImagePropertiesAsync().AsTask().Result;
		//			_dateTaken = props.DateTaken.ToString("yyyy-MM-dd");
		//		}
		//		return _dateTaken;

		//	}
		//}

	}
}
