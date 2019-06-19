using System.Windows;
using System.Windows.Controls;
using ThetaNetCore.Wifi;
using System.Windows.Threading;
using System;
using System.Threading.Tasks;
using System.Text;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// Home for the camera control
	/// </summary>
	public partial class CameraCtrl : UserControl
	{
		private ThetaWifiConnect _theta = new ThetaWifiConnect();

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
		}

		/// <summary>
		/// One of radio button is checked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			ctrlTakePict.TogglePreview(sender == radioTakePict);
			if (sender == radioPhotos)
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
				{
					await ctrlPhoto.ReloadAllFiles(false);
				}));

			}
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
