using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ThetaNetCoreApp.Info;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaNetCoreApp.Controls.Camera
{
	public sealed partial class PhotoCard : UserControl
	{
		#region events
		public event Action<FileEntryWrapper> DownloadRequested = null;
		public event Action<FileEntryWrapper> CancelRequested = null;
		#endregion

		public Boolean IsInEditMode
		{
			get { return (Boolean)GetValue(IsInEditModeProperty); }
			set { SetValue(IsInEditModeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsInEditMode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsInEditModeProperty =
			DependencyProperty.Register("IsInEditMode", typeof(Boolean), typeof(PhotoCard), new PropertyMetadata(false, OnIsSelectedChanged));

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PhotoCard)d).UpdateUIs();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public PhotoCard()
		{
			this.InitializeComponent();

			DataContextChanged += PhotoCard_DataContextChanged;
		}

		/// <summary>
		/// Data context changed event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PhotoCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var wrapper = e.NewValue as FileEntryWrapper;
			if (wrapper == null)
				return;

			wrapper.PropertyChanged += Val_PropertyChanged;
			UpdateUIs();
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
						if (!this.Dispatcher.CheckAccess())
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
			UpdateUIs();
		}

		/// <summary>
		/// Decide ctrl to show among download, progress, and completed image
		/// </summary>
		/// <param name="wrapper"></param>
		private void UpdateUIs()
		{
			var wrapper = this.DataContext as FileEntryWrapper;
			if (wrapper == null)
				return;

			System.Diagnostics.Debug.WriteLine("progress:" + wrapper.DownloadProgress);
			toggleChecked.Visibility = IsInEditMode ? Visibility.Visible : Visibility.Collapsed;
			btnDownload.Visibility = !IsInEditMode && wrapper.DownloadStatus == DOWNLOAD_STATUS.NOT_DOWNLOADED ? Visibility.Visible : Visibility.Collapsed;
			btnCancel.Visibility = !IsInEditMode && wrapper.DownloadStatus == DOWNLOAD_STATUS.WAINTING ? Visibility.Visible : Visibility.Collapsed;
			if (wrapper.DownloadStatus == DOWNLOAD_STATUS.DOWNLOADING)
			{
				_progress.Visibility = Visibility.Visible;
				_progress.Value = wrapper.DownloadProgress;
			}
			else
			{
				//_progress.Visibility = Visibility.Collapsed;
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
