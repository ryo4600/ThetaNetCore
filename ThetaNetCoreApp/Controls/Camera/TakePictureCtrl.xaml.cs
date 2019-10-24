using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThetaNetCore.Wifi;
using ThetaNetCoreApp.Resources;

namespace ThetaNetCoreApp.Controls.Camera
{
	/// <summary>
	/// Control to preview camera image and take photos
	/// </summary>
	public partial class TakePictureCtrl : UserControl
	{
		private ThetaWifiConnect _theta = null;
		CameraSettingsWnd _settingsWnd = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public TakePictureCtrl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Set Theta control instance
		/// </summary>
		/// <param name="theta"></param>
		public void SetTheta(ThetaWifiConnect theta)
		{
			_theta = theta;

			_theta.ImageReady += ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted += ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed += ThetaApi_OnTakePictureFailed;
		}

		/// <summary>
		/// Unloaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			_theta.ImageReady -= ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted -= ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed -= ThetaApi_OnTakePictureFailed;
		}

		/// <summary>
		/// Visibility changed event. Hide photo window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue)
			{
				// Collapsed
				CameraSharedInfo.Instance.RestartPreviewRequested -= OnRestartPreviewRequested;

				if (_settingsWnd != null && _settingsWnd.Visibility == Visibility.Visible)
					_settingsWnd.Visibility = Visibility.Collapsed;
			}
			else
			{
				// Visible
				CameraSharedInfo.Instance.RestartPreviewRequested += OnRestartPreviewRequested;
				UpdateStatusTexts();
			}
		}

		/// <summary>
		/// Update status texts, such as remaining photo, video, and etc...
		/// </summary>
		async private void UpdateStatusTexts()
		{
			var optionsParam = new GetOptionsParam() { RemainingPictures = true, RemainingVideoSeconds = true, RemainingSpace = true, TotalSpace = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);
			pnlRemainings.DataContext = options;
		}

		/// <summary>
		/// Restart of preview is required
		/// </summary>
		async private void OnRestartPreviewRequested()
		{
			if (!this.Dispatcher.CheckAccess())
			{
				await this.Dispatcher.BeginInvoke(new Action(() =>
				{
					OnRestartPreviewRequested();
				}));
				return;
			}
			if (!chkPreview.IsChecked.Value)
				return;

			_theta.StopLivePreview();
			await Task.Delay(1000);
			_theta.StartLivePreview();
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
			if (this._theta == null)
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
					String msg = AppStrings.Err_PreviewImage + "\n" + ex.Message;
					txtError.Text = msg;
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
			txtProgress.Text = AppStrings.Msg_TakingAPicture;
			pnlPrgress.Visibility = Visibility.Visible;
			await _theta.TakePictureAsync();
			txtProgress.Text = AppStrings.Msg_Processing;
		}

		/// <summary>
		/// Take picture completed
		/// </summary>
		/// <param name="fileUrl"></param>
		private void ThetaApi_OnTakePictureCompleted(string fileUrl)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(async delegate
			{
				pnlPrgress.Visibility = Visibility.Collapsed;

				var stream = await _theta.ThetaApi.GetImageAsync(fileUrl);
				thumbImg.Source = BitmapFrame.Create(stream);

				((Storyboard)this.FindResource("OpenThumbnail")).Begin();
				OnRestartPreviewRequested();
				UpdateStatusTexts();
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
		/// Close button for thumbnail is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnThumbClose_Click(object sender, RoutedEventArgs e)
		{
			((Storyboard)this.FindResource("CloseThumbnail")).Begin();

		}

		/// <summary>
		/// Clear button of error message is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnClearMessage_Click(object sender, RoutedEventArgs e)
		{
			txtError.Text = "";
		}

		/// <summary>
		/// Settings button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnSettings_Checked(object sender, RoutedEventArgs e)
		{
			if (_settingsWnd == null)
			{
				_settingsWnd = new CameraSettingsWnd();
				_settingsWnd.Owner = App.Current.MainWindow;
				_settingsWnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				_settingsWnd.SaveWindowPosition = true;
				_settingsWnd.SetTheta(_theta);
				_settingsWnd.IsVisibleChanged += (s2, e2) =>
				{
					if (!(bool)e2.NewValue)
						btnSettings.IsChecked = false;
				};
			}
			_settingsWnd.Visibility = btnSettings.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
