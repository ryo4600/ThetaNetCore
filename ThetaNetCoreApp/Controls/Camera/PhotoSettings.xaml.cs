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

		const int DISABLED_TIMER_VALUE = 65535;
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
			var optionsParam = new GetOptionsParam() { ShutterVolume = true, ExposureDelay = true, SleepDelay = true, OffDelay = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);

			// shutter volume
			sliderVolume.Value = options.ShutterVolume;

			// exposure delay (self timer)
			tglExposureDelay.IsChecked = options.ExposureDelay > 0;
			if (tglExposureDelay.IsChecked.Value)
				sliderSelfTimer.Value = options.ExposureDelay;

			// sleep delay
			tglSleepDelay.IsChecked = options.SleepDelay != DISABLED_TIMER_VALUE;
			if (tglSleepDelay.IsChecked.Value)
			{
				sliderSleep.Value = options.SleepDelay;
			}

			// off delay
			tglOffDelay.IsChecked = options.OffDelay != DISABLED_TIMER_VALUE;
			if (CommonCameraInfo.Instance.Info.ThetaModel >= InfoResponse.THETA_MODEL.V)
			{
				sliderOffDelay.Minimum = 600;
				sliderOffDelay.Maximum = 2592000;
				sliderOffDelay.SmallChange = 60;
				sliderOffDelay.LargeChange = 120;
				if (tglOffDelay.IsChecked.Value)
				{
					sliderOffDelay.Value = options.OffDelay;
				}
			}
			else
			{
				sliderOffDelay.Minimum = 30;
				sliderOffDelay.Maximum = 1800;
				sliderOffDelay.SmallChange = 5;
				sliderOffDelay.LargeChange = 10;
				if (tglOffDelay.IsChecked.Value)
				{
					sliderOffDelay.Value = options.OffDelay;
				}
			}

			_isInitialized = true;
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

			if ((int)e.NewValue != e.NewValue)
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
		/// Self timer is On
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tglExposureDelay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_isInitialized)
				return;

			var options = new OptionValues();
			if (tglExposureDelay.IsChecked.Value)
			{
				options.ExposureDelay = (int)sliderSelfTimer.Value;
			}
			else
			{
				options.ExposureDelay = 0;
			}
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();
		}

		/// <summary>
		/// Self timer's value changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderSelfTimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_isInitialized || !sliderSelfTimer.IsEnabled)
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
		/// Tiggle sleep. Off means it does not go into sleep
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tglSleepDelay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_isInitialized)
				return;

			var options = new OptionValues();
			if (tglSleepDelay.IsChecked.Value)
			{
				options.SleepDelay = (int)sliderSleep.Value;
			}
			else
			{
				options.SleepDelay = DISABLED_TIMER_VALUE;
			}
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();
		}

		/// <summary>
		/// Sleep timer value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderSleep_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_isInitialized || !sliderSleep.IsEnabled)
				return;

			if ((int)e.NewValue != e.NewValue)
			{
				var newVal = (int)e.NewValue;
				sliderSleep.Value = (newVal + 5) / 10 * 10 - 5;
				return;
			}

			var options = new OptionValues();
			options.SleepDelay = (int)e.NewValue;
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();
		}

		/// <summary>
		/// Toggle Off delay. False means camera does not power off itself
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tglOffDelay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_isInitialized)
				return;

			var options = new OptionValues();
			if (tglOffDelay.IsChecked.Value)
			{
				options.OffDelay = (int)sliderOffDelay.Value * 60;
			}
			else
			{
				options.OffDelay = DISABLED_TIMER_VALUE;
			}
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
			if (!_isInitialized || !sliderOffDelay.IsEnabled)
				return;

			int multipleOf = 5;
			int dividedBy = 10;
			if (CommonCameraInfo.Instance.Info.ThetaModel >= InfoResponse.THETA_MODEL.V)
			{
				multipleOf = 60;
				dividedBy = 100;
			}

			int newVal = (int)e.NewValue;
			if ((int)e.NewValue != e.NewValue)
			{
				sliderOffDelay.Value = (newVal + multipleOf) / dividedBy * dividedBy - multipleOf;
				return;
			}



			var options = new OptionValues();
			options.OffDelay = newVal;
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();
		}

	}
}
