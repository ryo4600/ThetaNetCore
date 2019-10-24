using ExifLib;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using ThetaNetCore.Common;
using ThetaNetCoreApp.Utils;

namespace ThetaNetCoreApp.Info
{
	public class ImageFileWrapper : BindableBase
	{
		private bool _isChecked = false;
		public bool IsChecked
		{
			get => _isChecked;
			set { SetProperty<bool>(ref _isChecked, value); }
		}

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
					_thumbImage = ImageTools.LoadThumbnail(_file.FullName);
				}

				return _thumbImage;
			}
			set { SetProperty<BitmapSource>(ref _thumbImage, value); }
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
	}
}
