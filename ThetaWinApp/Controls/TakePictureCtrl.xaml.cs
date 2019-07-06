using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThetaNetCore.Wifi;
using ThetaWinApp.Resources;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// Control to preview camera image and take photos
	/// </summary>
	public partial class TakePictureCtrl : UserControl
	{
		private ThetaWifiConnect _theta = null;

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
			_theta.ImageReady -= ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted -= ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed -= ThetaApi_OnTakePictureFailed;
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
					ShowError(msg);
				}

			}));
		}

		/// <summary>
		/// Show error message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		private void ShowError(String message, String title = "Error")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
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

	}
}
