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
using ThetaNetCoreApp.Resources;
using static ThetaNetCore.Wifi.InfoResponse;

namespace ThetaNetCoreApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for PhotoSettings.xaml
	/// </summary>
	public partial class ImageQualitySettings : UserControl
	{
		#region Members and Properties
		private enum EXPOSURE_PROGRAM { MANUAL = 1, NORMAL = 2, APERTURE = 3, SHUTTER = 4, ISO = 9 };

		private ThetaWifiConnect _theta = null;
		bool _isInitialized = false;

		/// <summary>
		/// This timer is to prevent from updating values in short time interval.
		/// Executes if 1 second has passed, after the last change.
		/// </summary>
		DispatcherTimer _delayUpdateTimer = null;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public ImageQualitySettings()
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
			var optionsParam = new GetOptionsParam() { ColorTemperature = true, WhiteBalance = true, ExposureProgram = true, Aperture = true, ShutterSpeed = true, Iso = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);

			#region White Balance
			KeyValuePair<string, string>[] WbValues = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(AppStrings.Title_WB_Auto, "auto"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_DayLight, "daylight"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_Shade, "shade"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_Cloudy, "cloudy-daylight"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_Incandescent, "incandescent"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_WarmWhite, "_warmWhiteFluorescent"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_DayLightFluorescent, "_dayLightFluorescent"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_WhiteFluorescent, "_dayWhiteFluorescent"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_Fluorescent, "fluorescent"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_BulbFluorescent, "_bulbFluorescent"),
				new KeyValuePair<string, string>(AppStrings.Title_WB_ColorTemperature, "_colorTemperature"),
			};
			cmbWhiteBalance.ItemsSource = WbValues;
			for (int i = 0; i < WbValues.Length; i++)
			{
				if (WbValues[i].Value == options.WhiteBalance)
				{
					cmbWhiteBalance.SelectedIndex = i;
					break;
				}
			}
			#endregion

			#region Color Temperature
			sliderColorTemperature.Value = options.ColorTemperature;
			#endregion

			#region Exposure program
			List<KeyValuePair<string, EXPOSURE_PROGRAM>> programValues = new List<KeyValuePair<string, EXPOSURE_PROGRAM>>();
			programValues.Add(new KeyValuePair<string, EXPOSURE_PROGRAM>(AppStrings.Title_ExposureManual, EXPOSURE_PROGRAM.MANUAL));
			programValues.Add(new KeyValuePair<string, EXPOSURE_PROGRAM>(AppStrings.Title_ExposureNormal, EXPOSURE_PROGRAM.NORMAL));
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.Z1)
				programValues.Add(new KeyValuePair<string, EXPOSURE_PROGRAM>(AppStrings.Title_ExposureAperture, EXPOSURE_PROGRAM.APERTURE));
			programValues.Add(new KeyValuePair<string, EXPOSURE_PROGRAM>(AppStrings.Title_ExposureShutter, EXPOSURE_PROGRAM.SHUTTER));
			programValues.Add(new KeyValuePair<string, EXPOSURE_PROGRAM>(AppStrings.Title_ExposureISO, EXPOSURE_PROGRAM.ISO));

			cmbExposureProgram.ItemsSource = programValues;
			for (int i = 0; i < programValues.Count; i++)
			{
				if ((int)programValues[i].Value == options.ExposureProgram)
				{
					cmbExposureProgram.SelectedIndex = i;
					break;
				}
			}
			#endregion

			#region Aperture
			KeyValuePair<string, float>[] apertureValues = null;
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.Z1)
			{
				apertureValues = new KeyValuePair<string, float>[]
				{
					new KeyValuePair<string, float>("2.1", 2.1f),
					new KeyValuePair<string, float>("3.5", 3.5f),
					new KeyValuePair<string, float>("5.6", 5.6f),
				};
			}
			else
			{
				apertureValues = new KeyValuePair<string, float>[]
				{
					new KeyValuePair<string, float>("2.0", 2.0f)
				};
			}
			cmbAperture.ItemsSource = apertureValues;

			for (int i = 0; i < apertureValues.Length; i++)
			{
				if (apertureValues[i].Value == options.Aperture)
				{
					cmbAperture.SelectedIndex = i;
					break;
				}
			}
			#endregion

			#region ShutterSpeed level
			List<KeyValuePair<string, float>> speedValues = new List<KeyValuePair<string, float>>();
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.V)
			{
				speedValues.Add(new KeyValuePair<string, float>("1/25000", 0.00004f));
				speedValues.Add(new KeyValuePair<string, float>("1/20000", 0.00005f));
				speedValues.Add(new KeyValuePair<string, float>("1/16000", 0.0000625f));
				speedValues.Add(new KeyValuePair<string, float>("1/12500", 0.00008f));
				speedValues.Add(new KeyValuePair<string, float>("1/10000", 0.00001f));
			}
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.SC)
			{
				speedValues.Add(new KeyValuePair<string, float>("1/8000", 0.000125f));
			}
			speedValues.Add(new KeyValuePair<string, float>("1/6400", 0.00015625f));
			speedValues.Add(new KeyValuePair<string, float>("1/5000", 0.0002f));
			speedValues.Add(new KeyValuePair<string, float>("1/4000", 0.00025f));
			speedValues.Add(new KeyValuePair<string, float>("1/3200", 0.0003125f));
			speedValues.Add(new KeyValuePair<string, float>("1/2500", 0.0004f));
			speedValues.Add(new KeyValuePair<string, float>("1/2000", 0.0005f));
			speedValues.Add(new KeyValuePair<string, float>("1/1600", 0.000625f));
			speedValues.Add(new KeyValuePair<string, float>("1/1250", 0.0008f));
			speedValues.Add(new KeyValuePair<string, float>("1/1000", 0.001f));
			speedValues.Add(new KeyValuePair<string, float>("1/800", 0.00125f));
			speedValues.Add(new KeyValuePair<string, float>("1/640", 0.0015625f));
			speedValues.Add(new KeyValuePair<string, float>("1/500", 0.002f));
			speedValues.Add(new KeyValuePair<string, float>("1/400", 0.0025f));
			speedValues.Add(new KeyValuePair<string, float>("1/320", 0.003125f));
			speedValues.Add(new KeyValuePair<string, float>("1/250", 0.004f));
			speedValues.Add(new KeyValuePair<string, float>("1/200", 0.005f));
			speedValues.Add(new KeyValuePair<string, float>("1/160", 0.00625f));
			speedValues.Add(new KeyValuePair<string, float>("1/125", 0.008f));
			speedValues.Add(new KeyValuePair<string, float>("1/100", 0.01f));
			speedValues.Add(new KeyValuePair<string, float>("1/80", 0.0125f));
			speedValues.Add(new KeyValuePair<string, float>("1/60", 0.01666666f));
			speedValues.Add(new KeyValuePair<string, float>("1/50", 0.02f));
			speedValues.Add(new KeyValuePair<string, float>("1/40", 0.025f));
			speedValues.Add(new KeyValuePair<string, float>("1/30", 0.03333333f));
			speedValues.Add(new KeyValuePair<string, float>("1/25", 0.04f));
			speedValues.Add(new KeyValuePair<string, float>("1/20", 0.05f));
			speedValues.Add(new KeyValuePair<string, float>("1/15", 0.06666666f));
			speedValues.Add(new KeyValuePair<string, float>("1/13", 0.07692307f));
			speedValues.Add(new KeyValuePair<string, float>("1/10", 0.1f));
			speedValues.Add(new KeyValuePair<string, float>("1/8", 0.125f));
			speedValues.Add(new KeyValuePair<string, float>("1/6", 0.16666666f));
			speedValues.Add(new KeyValuePair<string, float>("1/5", 0.2f));
			speedValues.Add(new KeyValuePair<string, float>("1/4", 0.25f));
			speedValues.Add(new KeyValuePair<string, float>("1/3", 0.33333333f));
			speedValues.Add(new KeyValuePair<string, float>("1/2.5", 0.4f));
			speedValues.Add(new KeyValuePair<string, float>("1/2", 0.5f));
			speedValues.Add(new KeyValuePair<string, float>("1/1.6", 0.625f));
			speedValues.Add(new KeyValuePair<string, float>("1", 1f));
			speedValues.Add(new KeyValuePair<string, float>("1.3", 1.3f));
			speedValues.Add(new KeyValuePair<string, float>("1.6", 1.6f));
			speedValues.Add(new KeyValuePair<string, float>("2", 2f));
			speedValues.Add(new KeyValuePair<string, float>("2.5", 2.5f));
			speedValues.Add(new KeyValuePair<string, float>("3.2", 3.2f));
			speedValues.Add(new KeyValuePair<string, float>("4", 4f));
			speedValues.Add(new KeyValuePair<string, float>("5", 5f));
			speedValues.Add(new KeyValuePair<string, float>("6", 6f));
			speedValues.Add(new KeyValuePair<string, float>("8", 8f));
			speedValues.Add(new KeyValuePair<string, float>("10", 10f));
			speedValues.Add(new KeyValuePair<string, float>("13", 13f));
			speedValues.Add(new KeyValuePair<string, float>("15", 15f));
			speedValues.Add(new KeyValuePair<string, float>("20", 20f));
			speedValues.Add(new KeyValuePair<string, float>("25", 25f));
			speedValues.Add(new KeyValuePair<string, float>("30", 30f));
			speedValues.Add(new KeyValuePair<string, float>("60", 60f));
			cmbSpeed.ItemsSource = speedValues;

			for (int i = 0; i < speedValues.Count; i++)
			{
				if (speedValues[i].Value == options.ShutterSpeed)
				{
					cmbSpeed.SelectedIndex = i;
					break;
				}
			}
			#endregion

			#region ISO
			List<int> isoValues = new List<int>(new int[] { 100, 125, 160, 200, 250, 320, 400, 500, 640, 800, 1000, 1250, 1600 });
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.V)
			{
				isoValues.Insert(0, 80);
				isoValues.AddRange(new int[] { 2000, 2500, 3200, 4000, 5000 });
			}
			if (CameraSharedInfo.Instance.Info.ThetaModel == THETA_MODEL.V)
			{
				isoValues.Insert(0, 64);
				isoValues.Add(6000);
			}
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.Z1)
			{
				isoValues.Add(6400);
			}

			cmbIso.ItemsSource = isoValues;

			for (int i = 0; i < isoValues.Count; i++)
			{
				if (isoValues[i] == options.ISO)
				{
					cmbIso.SelectedIndex = i;
					break;
				}
			}
			#endregion

			_isInitialized = true;
		}

		/// <summary>
		/// White balance setting is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WhiteBalance_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			pnlWB.IsEnabled = ((KeyValuePair<string, string>)cmbWhiteBalance.SelectedItem).Key == AppStrings.Title_WB_ColorTemperature;

			if (!_isInitialized)
				return;

			var options = new OptionValues();
			options.WhiteBalance = cmbWhiteBalance.SelectedValue.ToString();
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
			}));
		}

		/// <summary>
		/// Value of Colore temperature is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sliderColorTemperature_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_isInitialized)
				return;

			if ((int)e.NewValue != e.NewValue)
			{
				sliderColorTemperature.Value = ((int)e.NewValue / 100) * 100;
				return;
			}

			var options = new OptionValues();
			options.ColorTemperature = (int)e.NewValue;
			_delayUpdateTimer.Stop();
			_delayUpdateTimer.Tag = options;
			_delayUpdateTimer.Start();

		}

		/// <summary>
		/// Setting of Exposure program is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExposureProgram_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var newProgram = (EXPOSURE_PROGRAM)cmbExposureProgram.SelectedValue;
			pnlAperture.IsEnabled = newProgram == EXPOSURE_PROGRAM.MANUAL || newProgram == EXPOSURE_PROGRAM.APERTURE;
			pnlShutter.IsEnabled = newProgram == EXPOSURE_PROGRAM.MANUAL || newProgram == EXPOSURE_PROGRAM.SHUTTER;
			pnlIso.IsEnabled = newProgram == EXPOSURE_PROGRAM.MANUAL || newProgram == EXPOSURE_PROGRAM.ISO;

			if (!_isInitialized)
				return;

			var options = new OptionValues();
			options.ExposureProgram = (int)cmbExposureProgram.SelectedValue;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
			}));

		}

		/// <summary>
		/// Selection changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Aperture_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_isInitialized)
				return;

			var options = new OptionValues();
			options.Aperture = (float)cmbAperture.SelectedValue;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
			}));
		}

		/// <summary>
		/// Shutter sppeds' selection changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShutterSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_isInitialized)
				return;

			var options = new OptionValues();
			options.ShutterSpeed = (float)cmbSpeed.SelectedValue;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
			}));
		}

		/// <summary>
		/// Setting of ISO sensitivity has been changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Iso_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_isInitialized)
				return;

			var options = new OptionValues();
			options.ISO = (int)cmbIso.SelectedValue;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
			}));
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

	}
}
