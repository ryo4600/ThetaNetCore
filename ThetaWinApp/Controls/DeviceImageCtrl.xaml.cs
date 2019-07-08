using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ThetaWinApp.Info;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaWinApp.Controls
{
	public sealed partial class DeviceImageCtrl : UserControl
	{
		#region events
		public event Action<FileEntryWrapper> DownloadRequested = null;
		public event Action<FileEntryWrapper> CancelRequested = null;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceImageCtrl()
		{
			this.InitializeComponent();

			DataContextChanged += DeviceImageCtrl_DataContextChanged;
		}

		/// <summary>
		/// Data context changed event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeviceImageCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var wrapper = e.NewValue as FileEntryWrapper;
			if (wrapper == null)
				return;

			wrapper.PropertyChanged += Val_PropertyChanged;
			UpdateUIs(wrapper);
		}

		/// <summary>
		/// Property value changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Val_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(FileEntryWrapper.DownloadProgress):
					{
						if(!this.Dispatcher.CheckAccess())
						{
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
							{
								UpdateProgress();
							}));
							return;
						}
						UpdateProgress();
					}
					return;
			}
		}

		/// <summary>
		/// Update progress bar and buttons
		/// </summary>
		private void UpdateProgress()
		{
			var wrapper = this.DataContext as FileEntryWrapper;
			UpdateUIs(wrapper);
		}

		/// <summary>
		/// Decide ctrl to show among download, progress, and completed image
		/// </summary>
		/// <param name="wrapper"></param>
		private void UpdateUIs(FileEntryWrapper wrapper)
		{
			if (wrapper == null)
				return;

			btnDownload.Visibility = wrapper.DownloadStatus == DOWNLOAD_STATUS.NOT_DOWNLOADED ? Visibility.Visible : Visibility.Collapsed;
			btnCancel.Visibility = wrapper.DownloadStatus == DOWNLOAD_STATUS.WAINTING ? Visibility.Visible : Visibility.Collapsed;
			if (wrapper.DownloadStatus == DOWNLOAD_STATUS.DOWNLOADING)
			{
				_progress.Visibility = Visibility.Visible;
				_progress.Value = wrapper.DownloadProgress;
			}
			else
			{
				_progress.Visibility = Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Download button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDownload_Click(object sender, RoutedEventArgs e)
		{
			DownloadRequested?.Invoke(this.DataContext as FileEntryWrapper);
		}

		/// <summary>
		/// Cancel button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this.DataContext as FileEntryWrapper);
		}
	}
}
