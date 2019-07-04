using System;
using System.Windows.Media.Imaging;
using ThetaNetCore.Common;
using ThetaNetCore.Wifi;

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

		public String SimpleDate
		{
			get { return Data.DateTime.Substring(0, 10).Replace(":", "-"); }
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
