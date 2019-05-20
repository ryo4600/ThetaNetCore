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
using ThetaNetSample.Properties;

namespace ThetaNetSample
{
	/// <summary>
	/// Main window
	/// </summary>
	public partial class MainWindow : Window
	{
		ThetaWifiConnect _theta = new ThetaWifiConnect();

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// General Event Handler<br/>
		/// Windows loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			if(settings.FormWidth > 0)
			{
				this.Width = settings.FormWidth;
				this.Height = settings.FormHeight;
			}

			_theta.ImageReady += ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted += ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed += ThetaApi_OnTakePictureFailed;
		}

		/// <summary>
		/// General Event Handler <br />
		/// Window is closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var settings = Settings.Default;
			if(this.WindowState == WindowState.Normal)
			{
				settings.FormWidth = this.Width;
				settings.FormHeight = this.Height;
			}
			settings.Save();

			_theta.ImageReady -= ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted -= ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed -= ThetaApi_OnTakePictureFailed;

		}

		/// <summary>
		/// Common method to create a message when an exception is caught.
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		private String HandleException(Exception ex)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(ex.Message);
			if (ex.InnerException != null)
			{
				builder.AppendLine(ex.InnerException.Message);
			}

			return builder.ToString();

		}

		/// <summary>
		/// Check Connection button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnCheck_Click(object sender, RoutedEventArgs e)
		{
			string result;
			try
			{
				await _theta.CheckConnection();
				pnlController.IsEnabled = true;
				result = "Connection OK";
			}
			catch (ThetaWifiApiException apiex)
			{
				// Theta API erros... 
				result = HandleException(apiex);
				pnlController.IsEnabled = false;
			}
			catch (ThetaWifiConnectException connex)
			{
				// Connecton error...
				result = HandleException(connex);
				pnlController.IsEnabled = false;
			}

			txtOutput.Text += result + "\n";
		}

		/// <summary>
		/// Settings button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnSettings_Click(object sender, RoutedEventArgs e)
		{
			String result;
			try
			{
				var status = await _theta.ThetaApi.StateAsync();
				result = JsonUtil.ToSring(status);
			}
			catch (ThetaWifiApiException apiex)
			{
				result = HandleException(apiex);
			}
			txtOutput.Text += result + "\n";
		}

		/// <summary>
		/// Tab selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tabSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selItem = ((TabControl)sender).SelectedItem;
			if (selItem == tabPreview)
				TogglePreview(true);
			else if(selItem == tabFiles)
			{
				if (lstFiles.DataContext == null)
					ReloadAllFiles();
			}
			else
				TogglePreview(false);
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
			if (!pnlController.IsEnabled)
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
					HandleException(ex);
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
		/// <param name="obj"></param>
		private void ThetaApi_OnTakePictureCompleted(string obj)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				pnlPrgress.Visibility = Visibility.Collapsed;
			}));
		}

		/// <summary>
		/// Take picture failed
		/// </summary>
		/// <param name="obj"></param>
		private void ThetaApi_OnTakePictureFailed(Exception obj)
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

			// Load file images ... 
			// Usually you want to delay this step
			for (var i = 0; i < entries.Count; i++)
			{
				var anEntry = entries[i];
				var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 1, Detail = true, StartPosition = i };
				try
				{
					var res = await _theta.ThetaApi.ListFilesAsync(param);
					if (res.Entries.Length == 0)
						continue;
					entries[i] = res.Entries[0];

				}
				catch
				{
				}
			}
			lstFiles.DataContext = entries;

		}
	}
}
