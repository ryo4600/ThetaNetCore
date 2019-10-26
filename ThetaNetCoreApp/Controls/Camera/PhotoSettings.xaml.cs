using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ThetaNetCore.Wifi;

namespace ThetaNetCoreApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for PhotoSettings.xaml
	/// </summary>
	public partial class PhotoSettings : UserControl
	{
		#region Members and Properties
		private ThetaWifiConnect _theta = null;
		bool _isInitialized = false;

		/// <summary>
		/// This timer is to prevent from updating values in short time interval.
		/// Executes if 1 second has passed, after the last change.
		/// </summary>
		DispatcherTimer _delayUpdateTimer = null;

		const int TIMER_DISABLED = 65535;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public PhotoSettings()
		{
			InitializeComponent();
			this.IsEnabled = false;
			_delayUpdateTimer = new DispatcherTimer();
			_delayUpdateTimer.Interval = new TimeSpan(0, 0, 1);
			_delayUpdateTimer.Tick += _delayUpdateTimer_Tick;

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

			InitControlsValues();
		}

		/// <summary>
		/// Initialize contorls bases
		/// </summary>
		async private void InitControlsValues()
		{
			var optionsParam = new GetOptionsParam() { ExposureDelay = true, ShutterVolume = true, SleepDelay = true, OffDelay = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);

			// exposure delay (self timer)
			sliderSelfTimer.Value = options.ExposureDelay;

			// shutter volume
			sliderVolume.Value = options.ShutterVolume;

			_isInitialized = true;
		}

		/// <summary>
		/// Self timer's value changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderSelfTimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_isInitialized)
				return;

			if ((int)e.NewValue != e.NewValue)
			{
				sliderSelfTimer.Value = (int)e.NewValue;
				return;
			}

			var options = new OptionValues();
			options.ExposureDelay = (int)e.NewValue;
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();

		}

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
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();
		}

		/// <summary>
		/// Update time ticked. Start loading file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _delayUpdateTimer_Tick(object sender, EventArgs e)
		{
			_delayUpdateTimer.Stop();
			var options = _delayUpdateTimer.Tag as OptionValues;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
			}));
		}

		/// <summary>
		/// Sleep timer value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderSleep_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_isInitialized)
				return;

			if ((int)e.NewValue != e.NewValue)
			{
				sliderVolume.Value = (int)e.NewValue;
				return;
			}

			UpdateSleepDelayValue();
		}

		/// <summary>
		/// Updates sleep delay values
		/// </summary>
		private void UpdateSleepDelayValue()
		{
			int newValue;
			if (tglSleepDelay.IsChecked.Value)
				newValue = TIMER_DISABLED;
			else
				newValue = (int)sliderSleep.Value;
			var options = new OptionValues();
			options.SleepDelay = newValue;
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();
		}

		/// <summary>
		/// Off timer value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderOff_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{

		}

		private void UpdateOffDelayValue()
		{

		}
	}
}
