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

		private int _downloadState = -1;
		/// <summary>
		/// 0: not downloaded, 100 : downloaded, in-between : download in progress
		/// </summary>
		public int DownloadState
		{
			get { return _downloadState; }
			set { SetProperty<int>(ref _downloadState, value); }
		}
		public const String STR_DOWNLOAD_STATE = "DownloadState";

		public int EntryNo { get; set; }
	}
}
