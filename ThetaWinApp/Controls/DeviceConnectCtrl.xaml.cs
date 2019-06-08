using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ThetaNetCore.Util;
using ThetaNetCore.Wifi;
using ThetaWinApp.Resources;
using tkMessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// Interaction logic for DeviceConnectCtrl.xaml
	/// </summary>
	public partial class DeviceConnectCtrl : UserControl
	{
		public event Action ConnectionEstablished = null;

		private ThetaWifiConnect _theta;
		public ThetaWifiConnect Theta { get => _theta; set => _theta = value; }

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceConnectCtrl()
		{
			InitializeComponent();
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
			MessageBox.Show(msg, AppStrings.Err_Connection_Title, MessageBoxButton.OK, MessageBoxImage.Error);
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
				pnlController.IsEnabled = false;
				busyIndicator.IsBusy = true;

				busyIndicator.BusyContent = AppStrings.Msg_TryConnecting;

				await _theta.CheckConnection();

				pnlController.IsEnabled = true;
				ConnectionEstablished?.Invoke();
			}
			catch (ThetaWifiApiException apiex)
			{
				// Theta API erros... 
				HandleError(apiex, AppStrings.Err_Connection_Title);

			}
			catch (ThetaWifiConnectException connex)
			{
				// Connecton error...
				HandleError(connex, AppStrings.Err_Connection_Title);
			}
			finally
			{
				busyIndicator.IsBusy = false;
			}
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
				txtOutput.Text += result + "\n";
			}
			catch (ThetaWifiApiException apiex)
			{
				HandleError(apiex);
			}
		}

	}
}
