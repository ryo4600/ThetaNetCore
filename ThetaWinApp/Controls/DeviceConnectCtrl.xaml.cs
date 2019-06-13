using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ThetaNetCore;
using ThetaNetCore.Util;
using ThetaNetCore.Wifi;
using ThetaWinApp.Properties;
using ThetaWinApp.Resources;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// Interaction logic for DeviceConnectCtrl.xaml
	/// </summary>
	public partial class DeviceConnectCtrl : UserControl
	{
		private ThetaWifiConnect _theta = new ThetaWifiConnect();

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceConnectCtrl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Loaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			txtSavePath.Text = settings.SavePath;

			_theta.ImageReady += ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted += ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed += ThetaApi_OnTakePictureFailed;

			UpdateDownloadButton();

		}

		/// <summary>
		/// Unloaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_theta.ImageReady -= ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted -= ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed -= ThetaApi_OnTakePictureFailed;

			var settings = Settings.Default;
			settings.SavePath = txtSavePath.Text;
			settings.Save();
		}

		/// <summary>
		/// Tab selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void tabSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selItem = ((TabControl)sender).SelectedItem;
			if (selItem == tabPreview)
				TogglePreview(true);
			else if (selItem == tabFiles)
			{
				if (lstFiles.DataContext == null)
					await ReloadAllFiles();
			}
			else
				TogglePreview(false);
		}

		/// <summary>
		/// Check Connection button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnCheck_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				pnlStatus.IsEnabled = false;
				pnlPrgress.Visibility = Visibility.Visible;
				pnlPrgress.DataContext = AppStrings.Msg_TryConnecting;

				await _theta.CheckConnection();

				pnlStatus.IsEnabled = true;
				tabPreview.Visibility = Visibility.Visible;

				pnlPrgress.Visibility = Visibility.Collapsed;

				await LoadStatus();

				await ShowMessage(AppStrings.Msg_ConnectionOK);
			}
			catch (ThetaWifiApiException apiex)
			{
				// Theta API erros... 
				pnlPrgress.Visibility = Visibility.Collapsed;
				HandleError(apiex, AppStrings.Err_Connection_Title);
				tabPreview.Visibility = Visibility.Collapsed;
			}
			catch (ThetaWifiConnectException connex)
			{
				// Connecton error...
				pnlPrgress.Visibility = Visibility.Collapsed;
				HandleError(connex, AppStrings.Err_Connection_Title);
				tabPreview.Visibility = Visibility.Collapsed;
			}
			finally
			{
			}
		}

		private async Task LoadStatus()
		{
			var status = await _theta.ThetaApi.StateAsync();
			pnlStatus.DataContext = status;
			//var keyVals = new List<KeyValuePair<string, object>>();
			//keyVals.Add(new KeyValuePair<string, object>(AppStrings.Title_BatteryLevel, status.State.BatteryLevel));
			//keyVals.Add(new KeyValuePair<string, object>(AppStrings.Title_BatteryStatus, status.State.BatteryStateFriendly));
			//keyVals.Add(new KeyValuePair<string, object>(AppStrings.Title_CaptureStatus, status.State.CaptureStatus));
			//keyVals.Add(new KeyValuePair<string, object>(AppStrings.Title_RecordedTime, status.State.RecordedTime));
			//keyVals.Add(new KeyValuePair<string, object>(AppStrings.Title_RecordableTime, status.State.RecordableTime));
			//keyVals.Add(new KeyValuePair<string, object>(AppStrings.Title_CameraError, status.State.CameraError));
			//pnlStatus.DataContext = keyVals;
		}

		/// <summary>
		/// Show message with a snack bar
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		private async Task ShowMessage(String msg)
		{
			SnackbarOne.Message.Content = msg;
			SnackbarOne.IsActive = true;
			await Task.Delay(2000);
			SnackbarOne.IsActive = false;
		}

		/// <summary>
		/// Common method to create a message when an exception is caught.
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		private void HandleError(Exception ex, String title = "Error")
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(ex.Message);
			if (ex.InnerException != null)
			{
				builder.AppendLine(ex.InnerException.Message);
			}

			var msg = builder.ToString();
			dlgErr.DataContext = msg;
			dlgErr.Visibility = Visibility.Visible;
		}

		/// <summary>
		/// Close button of error
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			dlgErr.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Settings button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var status = await _theta.ThetaApi.StateAsync();
				pnlStatus.DataContext = status;
			}
			catch (ThetaWifiApiException apiex)
			{
				HandleError(apiex);
			}
		}

		/// <summary>
		/// Preview check state is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void chkPreview_CheckedChanged(object sender, RoutedEventArgs e)
		{
			TogglePreview(chkPreview.IsChecked.Value);
		}

		/// <summary>
		/// Start / stop preview
		/// </summary>
		/// <param name="startPreview"></param>
		public void TogglePreview(bool startPreview)
		{
			// prevent an event before initialization
			if (tabPreview.Visibility != Visibility.Visible)
				return;

			if (chkPreview.IsChecked.Value && startPreview)
			{
				_theta.StartLivePreview();
			}
			else if (_theta.IsPreviewing)
			{
				_theta.StopLivePreview();

			}
		}

		/// <summary>
		/// Preview image is ready
		/// </summary>
		/// <param name="imgByteArray"></param>
		private void ThetaApi_ImageReady(byte[] imgByteArray)
		{
			// JPEG format data
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				try
				{
					var source = LoadImage(imgByteArray);
					imgPreview.Source = source;
				}
				catch (Exception ex)
				{
					// HandleException(ex);
				}

			}));
		}

		/// <summary>
		/// Load bitmap image from byte array
		/// </summary>
		/// <param name="imageData"></param>
		/// <returns></returns>
		private static BitmapImage LoadImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0)
				return null;

			var image = new BitmapImage();
			using (var mem = new MemoryStream(imageData))
			{
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();
			return image;
		}

		/// <summary>
		/// Take picture button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void BtnTakePicture_Click(object sender, RoutedEventArgs e)
		{
			pnlPrgress.Visibility = Visibility.Visible;
			await _theta.TakePictureAsync();
			txtProgress.Text = "Processing ... ";
		}

		/// <summary>
		/// Take picture completed
		/// </summary>
		/// <param name="fileName"></param>
		private void ThetaApi_OnTakePictureCompleted(string fileName)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				pnlPrgress.Visibility = Visibility.Collapsed;
			}));
		}

		/// <summary>
		/// Take picture failed
		/// </summary>
		/// <param name="ex"></param>
		private void ThetaApi_OnTakePictureFailed(Exception ex)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				pnlPrgress.Visibility = Visibility.Collapsed;
			}));
		}

		/// <summary>
		/// Refresh list button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnRefreshList_Click(object sender, RoutedEventArgs e)
		{
			await ReloadAllFiles();
		}

		/// <summary>
		/// Reload all files
		/// </summary>
		private async Task ReloadAllFiles()
		{
			var entries = new List<FileEntry>();
			// Get xx files in each iteration. EntryCount and StartPosition is important.
			while (true)
			{
				var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 50, StartPosition = entries.Count, Detail = false };
				var res = await _theta.ThetaApi.ListFilesAsync(param);
				foreach (var anEntry in res.Entries)
				{
					entries.Add(anEntry);
				}

				if (entries.Count >= res.TotalEntries)
					break;
			}

			lstFiles.DataContext = entries;

		}

		/// <summary>
		/// Selection of file list is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void lstFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var anEntry = lstFiles.SelectedItem as FileEntry;
			if (anEntry != null && anEntry.Thumbnail == null)
			{
				var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 1, Detail = true, StartPosition = lstFiles.SelectedIndex };
				try
				{
					var res = await _theta.ThetaApi.ListFilesAsync(param);
					if (res.Entries.Length != 0)
						anEntry = res.Entries[0];
				}
				catch
				{
				}
			}

			imgPictureView.DataContext = anEntry;
			UpdateDownloadButton();
		}


		/// <summary>
		/// Choose save path (...) is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnPath_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			if (!String.IsNullOrWhiteSpace(txtSavePath.Text))
				dlg.SelectedPath = txtSavePath.Text;

			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				txtSavePath.Text = dlg.SelectedPath;
			}

		}

		/// <summary>
		/// Save path text changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TxtSavePath_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDownloadButton();
		}

		/// <summary>
		/// Update state of the "download" button
		/// </summary>
		private void UpdateDownloadButton()
		{
			btnDownload.IsEnabled = Directory.Exists(txtSavePath.Text) && imgPictureView.DataContext != null;
		}

		/// <summary>
		/// Download button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnDownload_Click(object sender, RoutedEventArgs e)
		{
			var anEntry = imgPictureView.DataContext as FileEntry;

			// Get a read stream
			using (Stream stream = await _theta.ThetaApi.GetImageAsync(anEntry.FileUrl))
			{
				var path = Path.Combine(txtSavePath.Text, anEntry.Name);
				var size = anEntry.Size;
				var newFile = new FileInfo(path);
				using (var aStream = newFile.Open(FileMode.OpenOrCreate))
				{
					int readSize = 1000;
					var readBuffer = new byte[readSize];
					int totalRead = 0;
					for (int i = 0; i < (int)Math.Ceiling(size / (double)readSize); i++)
					{
						var numRead = await stream.ReadAsync(readBuffer, 0, readSize);

						aStream.Write(readBuffer, 0, numRead);
						totalRead += numRead;
					}
				}
			}
		}

	}
}
