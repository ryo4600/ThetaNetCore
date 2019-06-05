using System;
using System.Collections.Generic;
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
using ThetaNetCore.Util;
using ThetaNetCore.Wifi;

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

				ConnectionEstablished?.Invoke();
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


	}
}
