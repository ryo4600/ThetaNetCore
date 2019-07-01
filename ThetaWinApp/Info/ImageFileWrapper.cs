using ExifLib;
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
		public FileInfo File
		{
			get => _file;
			set
			{
				_file = value;
			}
		}

		private BitmapSource _thumbImage = null;
		public BitmapSource ThumbImage
		{
			get {
				if (_thumbImage == null)
				{
					_thumbImage = GetThumbnail(_file.FullName);
				}

				return _thumbImage;
			}
			set { SetProperty<BitmapSource>(ref _thumbImage, value); }
		}

		/// <summary>
		/// Getting thambnail. <br /> This one is fast.
		/// </summary>
		/// <param name="MediaUrl"></param>
		/// <returns></returns>
		static private BitmapSource GetThumbnail(String MediaUrl)
		{
			BitmapSource ret = null;
			BitmapMetadata meta = null;

			try
			{
				BitmapFrame frame = BitmapFrame.Create(
					new Uri(MediaUrl),
					BitmapCreateOptions.DelayCreation,
					BitmapCacheOption.None);

				if (frame.Thumbnail == null)
				{
					BitmapImage image = new BitmapImage();
					image.DecodePixelHeight = 90;
					image.BeginInit();
					image.UriSource = new Uri(MediaUrl);
					image.CacheOption = BitmapCacheOption.None;
					image.CreateOptions = BitmapCreateOptions.DelayCreation;
					image.EndInit();

					if (image.CanFreeze)
						image.Freeze();

					ret = image;
				}
				else
				{
					meta = frame.Metadata as BitmapMetadata;
					ret = frame.Thumbnail;
				}

			}
			catch (Exception ex)
			{
			}

			return ret;
		}

		public String FileName
		{
			get { return File.Name; }
		}

		DateTime? _dt = null;
		public DateTime DateTaken
		{
			get
			{
				if (_dt == null)
				{
					using (ExifReader reader = new ExifReader(_file.FullName))
					{
						// Extract the tag data using the ExifTags enumeration
						DateTime datePictureTaken;
						if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
														out datePictureTaken))
						{
							_dt = datePictureTaken;
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
