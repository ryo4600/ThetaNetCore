using System;
using System.Windows.Media.Imaging;
using ThetaNetCore.Common;
using ThetaNetCore.Wifi;
using ThetaWinApp.Utils;

namespace ThetaWinApp.Info
{
	public class FileEntryWrapper : BindableBase
	{
		private bool _isChecked = false;
		public bool IsChecked
		{
			get => _isChecked;
			set { SetProperty<bool>(ref _isChecked, value); }
		}

		public FileEntry Data { get; set; }

		private BitmapSource _thumbImage = null;
		public BitmapSource ThumbImage
		{
			get { return _thumbImage; }
			set { SetProperty<BitmapSource>(ref _thumbImage, value); }
		}

		private string _localThumbFile = "";
		public String LocalThumbFile
		{
			get { return _localThumbFile; }
			set
			{
				if (value == _localThumbFile)
					return;

				_localThumbFile = value;
				if (value != null)
				{
					this.ThumbImage = ImageTools.LoadThumbnail(_localThumbFile);
				}
			}
		}

		public String SimpleDate
		{
			get { return Data.DateTime.Substring(0, 10).Replace(":", "-"); }
		}

		public String TimeString
		{
			get { return Data.DateTime.Substring(12); }
		}

		private int _downloadProgress = -1;
		/// <summary>
		/// 0: not downloaded, 100 : downloaded, in-between : download in progress
		/// </summary>
		public int DownloadProgress
		{
			get { return _downloadProgress; }
			set { SetProperty<int>(ref _downloadProgress, value); }
		}

		/// <summary>
		/// Get the status of download
		/// </summary>
		public DOWNLOAD_STATUS DownloadStatus
		{
			get
			{
				switch(_downloadProgress)
				{
					case 100:
						return DOWNLOAD_STATUS.DOWNLOADED;
					case -1:
						return DOWNLOAD_STATUS.NOT_DOWNLOADED;
					case 0:
						return DOWNLOAD_STATUS.WAINTING;
					default:
						return DOWNLOAD_STATUS.DOWNLOADING;
				}
			}
			set
			{
				switch (value)
				{
					case DOWNLOAD_STATUS.NOT_DOWNLOADED:
						_downloadProgress = -1;
						break;
					case DOWNLOAD_STATUS.WAINTING:
						_downloadProgress = 0;
						break;
					default:
						// Unsupported
						break;
				}

			}
		}

		public int EntryNo { get; set; }
	}

	public enum DOWNLOAD_STATUS { NOT_DOWNLOADED, WAINTING, DOWNLOADING, DOWNLOADED };
}
