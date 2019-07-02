using System;
using System.Windows;
using System.Windows.Controls;
using ThetaWinApp.Info;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaWinApp.Controls
{
	public sealed partial class DeviceImageCtrl : UserControl
	{
		#region events
		public event Action<FileEntryWrapper> DownloadRequested = null;
		public event Action<FileEntryWrapper> DeleteRequested = null;
		#endregion

		//public bool ShowButtons
		//{
		//	get { return (bool)GetValue(ShowButtonsProperty); }
		//	set
		//	{
		//		SetValue(ShowButtonsProperty, value);
		//		ButtonArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		//	}
		//}

		//// Using a DependencyProperty as the backing store for ShowButtons.  This enables animation, styling, binding, etc...
		//public static readonly DependencyProperty ShowButtonsProperty =
		//	DependencyProperty.Register("ShowButtons", typeof(bool), typeof(DeviceImageCtrl), new PropertyMetadata(true));

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceImageCtrl()
		{
			this.InitializeComponent();

			DataContextChanged += DeviceImageCtrl_DataContextChanged;
		}

		private void DeviceImageCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var wrapper = e.NewValue as FileEntryWrapper;
			if (wrapper == null)
				return;

			wrapper.PropertyChanged += Val_PropertyChanged;
			UpdateButtonState(wrapper);
		}

		private void Val_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case FileEntryWrapper.STR_DOWNLOAD_STATE:
					{
						var wrapper = this.DataContext as FileEntryWrapper;
						UpdateButtonState(wrapper);
					}
					return;
			}

		}

		/// <summary>
		/// Decide ctrl to show among download, progress, and completed image
		/// </summary>
		/// <param name="wrapper"></param>
		private void UpdateButtonState(FileEntryWrapper wrapper)
		{
			//if (wrapper == null)
			//	return;

			//switch (wrapper.DownloadState)
			//{
			//	case -1:
			//		_btnDownload.Visibility = Visibility.Visible;
			//		_imgCompleted.Visibility = Visibility.Collapsed;
			//		_progress.Visibility = Visibility.Collapsed;
			//		break;
			//	case 100:
			//		_btnDownload.Visibility = Visibility.Collapsed;
			//		_imgCompleted.Visibility = Visibility.Visible;
			//		_progress.Visibility = Visibility.Collapsed;
			//		break;
			//	default:
			//		_btnDownload.Visibility = Visibility.Collapsed;
			//		_imgCompleted.Visibility = Visibility.Collapsed;
			//		_progress.Visibility = Visibility.Visible;
			//		_progress.Value = wrapper.DownloadState;
			//		break;
			//}
		}

		/// <summary>
		/// Download button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDownload_Click(object sender, RoutedEventArgs e)
		{
			if (DownloadRequested != null)
				DownloadRequested(this.DataContext as FileEntryWrapper);
		}

		/// <summary>
		/// Delete button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (DeleteRequested != null)
				DeleteRequested(this.DataContext as FileEntryWrapper);
		}

	}
}
