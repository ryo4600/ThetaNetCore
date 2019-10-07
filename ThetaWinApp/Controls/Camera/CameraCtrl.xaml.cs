using System.Windows;
using System.Windows.Controls;
using ThetaNetCore.Wifi;
using System.Windows.Threading;
using System;
using System.Threading.Tasks;
using System.Text;
using ThetaWinApp.Resources;

namespace ThetaWinApp.Controls.Camera
{
	/// <summary>
	/// Home for the camera control
	/// </summary>
	public partial class CameraCtrl : UserControl
	{
		private ThetaWifiConnect _theta = new ThetaWifiConnect();
		CameraSettingsWnd _settingsWnd = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public CameraCtrl()
		{
			InitializeComponent();
			this.Loaded += (sender, e) =>
			{
				ctrlConnect.SetTheta(_theta);
				ctrlTakePict.SetTheta(_theta);
				ctrlPhoto.SetTheta(_theta);

				ctrlConnect.OnConnectionReady += (connected) =>
				{
					radioTakePict.IsEnabled = radioPhotos.IsEnabled = connected;
				};
				ctrlConnect.OnShowMessage += ShowMessage;
				ctrlConnect.OnShowError += ShowError;
			};

			_theta.OnPreviewTerminated += (wifiEx) =>
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
				{
					var msg = AppStrings.Err_PreviewImage + '\n' + wifiEx.Message;
					MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					// Theta is disconnected 
					radioConnect.IsChecked = true;
					ctrlConnect.ClearBinding();
					radioTakePict.IsEnabled = radioPhotos.IsEnabled = false;
				}));
			};

			this.IsVisibleChanged += CameraCtrl_IsVisibleChanged;
		}

		/// <summary>
		/// Visibility changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CameraCtrl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue)
			{
				if (_settingsWnd != null && _settingsWnd.Visibility == Visibility.Visible)
					_settingsWnd.Visibility = Visibility.Collapsed;
			}
		}

		/// <summary>
		/// One of radio button is checked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			ctrlTakePict?.TogglePreview(sender == radioTakePict);
			if (sender == radioPhotos)
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
				{
					await ctrlPhoto.ReloadAllFilesAsync(false);
				}));

			}
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


		/// <summary>
		/// Show message with a snack bar
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		private void ShowMessage(String msg)
		{
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
			{
				SnackbarOne.Message.Content = msg;
				SnackbarOne.IsActive = true;
				await Task.Delay(2000);
				SnackbarOne.IsActive = false;
			}));
		}

		/// <summary>
		/// Common method to create a message when an exception is caught.
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		private void ShowError(ShowErrorInfo errInfo)
		{
			StringBuilder builder = new StringBuilder();
			if (!String.IsNullOrEmpty(errInfo.Message))
				builder.AppendLine(errInfo.Message);

			Exception ex = errInfo.Exception;
			if (ex != null)
			{
				builder.AppendLine(ex.Message);
				if (ex.InnerException != null)
				{
					builder.AppendLine(ex.InnerException.Message);
				}
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
		private void btnErrClose_Click(object sender, RoutedEventArgs e)
		{
			dlgErr.Visibility = Visibility.Collapsed;
		}
	}

	public class ShowErrorInfo
	{
		public String Title { get; set; } = "Error";
		public String Message { get; set; }
		public Exception Exception { get; set; }
	}
}
