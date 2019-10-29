using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ThetaNetCore;
using ThetaNetCore.Util;
using ThetaNetCore.Wifi;
using ThetaNetCoreApp.Properties;
using ThetaNetCoreApp.Resources;

namespace ThetaNetCoreApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for DeviceConnectCtrl.xaml
	/// </summary>
	public partial class DeviceConnectCtrl : UserControl
	{
		private ThetaWifiConnect _theta = null;

		public event Action<bool> OnConnectionReady = null;
		public event Action<String> OnShowMessage = null;
		public event Action<ShowErrorInfo> OnShowError = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceConnectCtrl()
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
		}

		/// <summary>
		/// Check Connection button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnCheck_Click(object sender, RoutedEventArgs e)
		{
#if DEBUG
			if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				OnConnectionReady?.Invoke(true);
				return;
			}
#endif
			try
			{
				OnConnectionReady?.Invoke(false);

				pnlPrgress.Visibility = Visibility.Visible;
				pnlPrgress.DataContext = AppStrings.Msg_TryConnecting;

				await _theta.CheckConnection();

				pnlPrgress.Visibility = Visibility.Collapsed;

				await LoadStatusAndInfo();
				OnShowMessage?.Invoke(AppStrings.Msg_ConnectionOK);

				OnConnectionReady?.Invoke(true);

			}
			catch (ThetaWifiConnectException connex)
			{
				// Connecton error...
				pnlPrgress.Visibility = Visibility.Collapsed;
				HandleError(connex);
			}
			finally
			{
			}
		}

		/// <summary>
		/// Handle errors
		/// </summary>
		/// <param name="ex"></param>
		private void HandleError(Exception ex)
		{
			OnShowError?.Invoke(new ShowErrorInfo() { Exception = ex });
		}

		/// <summary>
		/// Load Status and Information from Theta
		/// </summary>
		/// <returns></returns>
		private async Task LoadStatusAndInfo()
		{
			var status = await _theta.ThetaApi.StateAsync();
			statusSection.DataContext = status;

			var info = await _theta.ThetaApi.InfoAsync();
			infoSection.DataContext = info;
			CommonCameraInfo.Instance.Info = info;

			if(info.ThetaModel >= InfoResponse.THETA_MODEL.V)
			{
				var plugins = await _theta.ThetaApi.ListPluginsAsync();
				pluginSection.DataContext = plugins?.Results?.Plugins;
				pluginSection.IsEnabled = true;
			}
			else
			{
				pluginSection.DataContext = null;
				pluginSection.IsEnabled = false;
			}
		}

		/// <summary>
		/// Clear binding.
		/// </summary>
		public void ClearBinding()
		{
			statusSection.DataContext = null;
			infoSection.DataContext = null;
		}

		///// <summary>
		///// Toggle "Foreground" is checked/unchecked
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//async private void ToggleForeground_Checked(object sender, RoutedEventArgs e)
		//{
		//	if(CameraSharedInfo.Instance.Info.ThetaModel == InfoResponse.THETA_MODEL.V)
		//	{
		//		var plugin = ((FrameworkElement)sender).DataContext as Plugin;
		//		await _theta.ThetaApi.SetPluginsAsync(plugin.PackageName);
		//	}
		//}

		///// <summary>
		///// Toggle "Running" is checked/unchecked
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//async private void ToggleRunning_Checked(object sender, RoutedEventArgs e)
		//{
		//	var plugin = ((FrameworkElement)sender).DataContext as Plugin;
		//	var action = ((ToggleButton)sender).IsChecked.Value ? PLUGIN_ACTION.START : PLUGIN_ACTION.STOP;
		//	if (CameraSharedInfo.Instance.Info.ThetaModel == InfoResponse.THETA_MODEL.V)
		//	{
		//		await _theta.ThetaApi.PluginControlAsync(action);
		//	}
		//	else
		//	{
		//		await _theta.ThetaApi.PluginControlAsync(action, plugin.PackageName);
		//	}
		//}
	}
}
