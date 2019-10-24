using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using ThetaNetCore.Wifi;
using static ThetaNetCore.Wifi.InfoResponse;

namespace ThetaNetCoreApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for PhotoSettings.xaml
	/// </summary>
	public partial class PhotoSettings : UserControl
	{
		#region Members and Properties
		private ThetaWifiConnect _theta = null;
		DispatcherTimer _delayLoadTimer = null;

		bool _isInitialized = false;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public PhotoSettings()
		{
			InitializeComponent();
			this.IsEnabled = false;
			_delayLoadTimer = new DispatcherTimer();
			_delayLoadTimer.Interval = new TimeSpan(0, 0, 1);
			_delayLoadTimer.Tick += _delayLoadTimer_Tick;

		}

		/// <summary>
		/// Set Theta instance
		/// </summary>
		/// <param name="theta"></param>
		internal void SetTheta(ThetaWifiConnect theta)
		{
			_theta = theta;
			this.IsEnabled = _theta != null;
		}

		/// <summary>
		/// Initialize controls first time to be seen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			InitThetaValues();
		}

		/// <summary>
		/// Initialize contorls bases
		/// </summary>
		async private void InitThetaValues()
		{
			var optionsParam = new GetOptionsParam() { ShutterSpeed = true, ShutterVolume = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);

			#region ShutterSpeed level
			//List<KeyValuePair<string, float>> speedValues = new List<KeyValuePair<string, float>>();
			//if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.V)
			//{
			//	speedValues.Add(new KeyValuePair<string, float>("1/25000", 0.00004f));
			//	speedValues.Add(new KeyValuePair<string, float>("1/20000", 0.00005f));
			//	speedValues.Add(new KeyValuePair<string, float>("1/16000", 0.0000625f));
			//	speedValues.Add(new KeyValuePair<string, float>("1/12500", 0.00008f));
			//	speedValues.Add(new KeyValuePair<string, float>("1/10000", 0.00001f));
			//}
			//if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.SC)
			//{
			//	speedValues.Add(new KeyValuePair<string, float>("1/8000", 0.000125f));
			//}
			//speedValues.Add(new KeyValuePair<string, float>("1/6400", 0.00015625f));
			//speedValues.Add(new KeyValuePair<string, float>("1/5000", 0.0002f));
			//speedValues.Add(new KeyValuePair<string, float>("1/4000", 0.00025f));
			//speedValues.Add(new KeyValuePair<string, float>("1/3200", 0.0003125f));
			//speedValues.Add(new KeyValuePair<string, float>("1/2500", 0.0004f));
			//speedValues.Add(new KeyValuePair<string, float>("1/2000", 0.0005f));
			//speedValues.Add(new KeyValuePair<string, float>("1/1600", 0.000625f));
			//speedValues.Add(new KeyValuePair<string, float>("1/1250", 0.0008f));
			//speedValues.Add(new KeyValuePair<string, float>("1/1000", 0.001f));
			//speedValues.Add(new KeyValuePair<string, float>("1/800", 0.00125f));
			//speedValues.Add(new KeyValuePair<string, float>("1/640", 0.0015625f));
			//speedValues.Add(new KeyValuePair<string, float>("1/500", 0.002f));
			//speedValues.Add(new KeyValuePair<string, float>("1/400", 0.0025f));
			//speedValues.Add(new KeyValuePair<string, float>("1/320", 0.003125f));
			//speedValues.Add(new KeyValuePair<string, float>("1/250", 0.004f));
			//speedValues.Add(new KeyValuePair<string, float>("1/200", 0.005f));
			//speedValues.Add(new KeyValuePair<string, float>("1/160", 0.00625f));
			//speedValues.Add(new KeyValuePair<string, float>("1/125", 0.008f));
			//speedValues.Add(new KeyValuePair<string, float>("1/100", 0.01f));
			//speedValues.Add(new KeyValuePair<string, float>("1/80", 0.0125f));
			//speedValues.Add(new KeyValuePair<string, float>("1/60", 0.01666666f));
			//speedValues.Add(new KeyValuePair<string, float>("1/50", 0.02f));
			//speedValues.Add(new KeyValuePair<string, float>("1/40", 0.025f));
			//speedValues.Add(new KeyValuePair<string, float>("1/30", 0.03333333f));
			//speedValues.Add(new KeyValuePair<string, float>("1/25", 0.04f));
			//speedValues.Add(new KeyValuePair<string, float>("1/20", 0.05f));
			//speedValues.Add(new KeyValuePair<string, float>("1/15", 0.06666666f));
			//speedValues.Add(new KeyValuePair<string, float>("1/13", 0.07692307f));
			//speedValues.Add(new KeyValuePair<string, float>("1/10", 0.1f));
			//speedValues.Add(new KeyValuePair<string, float>("1/8", 0.125f));
			//speedValues.Add(new KeyValuePair<string, float>("1/6", 0.16666666f));
			//speedValues.Add(new KeyValuePair<string, float>("1/5", 0.2f));
			//speedValues.Add(new KeyValuePair<string, float>("1/4", 0.25f));
			//speedValues.Add(new KeyValuePair<string, float>("1/3", 0.33333333f));
			//speedValues.Add(new KeyValuePair<string, float>("1/2.5", 0.4f));
			//speedValues.Add(new KeyValuePair<string, float>("1/2", 0.5f));
			//speedValues.Add(new KeyValuePair<string, float>("1/1.6", 0.625f));
			//speedValues.Add(new KeyValuePair<string, float>("1", 1f));
			//speedValues.Add(new KeyValuePair<string, float>("1.3", 1.3f));
			//speedValues.Add(new KeyValuePair<string, float>("1.6", 1.6f));
			//speedValues.Add(new KeyValuePair<string, float>("2", 2f));
			//speedValues.Add(new KeyValuePair<string, float>("2.5", 2.5f));
			//speedValues.Add(new KeyValuePair<string, float>("3.2", 3.2f));
			//speedValues.Add(new KeyValuePair<string, float>("4", 4f));
			//speedValues.Add(new KeyValuePair<string, float>("5", 5f));
			//speedValues.Add(new KeyValuePair<string, float>("6", 6f));
			//speedValues.Add(new KeyValuePair<string, float>("8", 8f));
			//speedValues.Add(new KeyValuePair<string, float>("10", 10f));
			//speedValues.Add(new KeyValuePair<string, float>("13", 13f));
			//speedValues.Add(new KeyValuePair<string, float>("15", 15f));
			//speedValues.Add(new KeyValuePair<string, float>("20", 20f));
			//speedValues.Add(new KeyValuePair<string, float>("25", 25f));
			//speedValues.Add(new KeyValuePair<string, float>("30", 30f));
			//speedValues.Add(new KeyValuePair<string, float>("60", 60f));
			//cmbSpeed.ItemsSource = speedValues;

			//for (int i = 0; i < speedValues.Count; i++)
			//{
			//	if (speedValues[i].Value == options.ShutterSpeed)
			//	{
			//		cmbSpeed.SelectedIndex = i;
			//		break;
			//	}
			//}
			#endregion

			// shutter volume
			sliderVolume.Value = options.ShutterVolume;

			_isInitialized = true;
		}

		///// <summary>
		///// Shutter sppeds' selection changed event
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//private void ShutterSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	if (!_isInitialized)
		//		return;

		//	var options = new OptionValues();
		//	options.ShutterSpeed = (float)cmbSpeed.SelectedValue;
		//	Task.Factory.StartNew(new Action(async () =>
		//	{
		//		await _theta.ThetaApi.SetOptionsAsync(options);
		//	}));
		//}

		/// <summary>
		/// Shutter volume's value changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_isInitialized)
				return;

			if((int)e.NewValue != e.NewValue)
			{
				sliderVolume.Value = (int)e.NewValue;
				return;
			}

			var options = new OptionValues();
			options.ShutterVolume = (int)e.NewValue;
			_delayLoadTimer.Stop();
			_delayLoadTimer.Tag = options;
			_delayLoadTimer.Start();
		}

		/// <summary>
		/// Update time ticked. Start loading file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _delayLoadTimer_Tick(object sender, EventArgs e)
		{
			_delayLoadTimer.Stop();
			var options = _delayLoadTimer.Tag as OptionValues;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
				System.Diagnostics.Debug.WriteLine("new option:" + options );
			}));
		}


	}
}
