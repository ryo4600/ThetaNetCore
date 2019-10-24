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
using ThetaNetCore.Wifi;
using static ThetaNetCore.Wifi.InfoResponse;

namespace ThetaWinApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for PhotoSettings.xaml
	/// </summary>
	public partial class ImageQualitySettings : UserControl
	{
		#region Members and Properties
		private ThetaWifiConnect _theta = null;

		bool _isInitialized = false;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public ImageQualitySettings()
		{
			InitializeComponent();
			this.IsEnabled = false;
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

			InitAperture();
		}

		/// <summary>
		/// Initialize contorls bases
		/// </summary>
		async private void InitAperture()
		{
			var optionsParam = new GetOptionsParam() { Aperture = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);

			// Aperture level
			KeyValuePair<string, float>[] apertureValues = null;
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.Z1)
			{
				apertureValues = new KeyValuePair<string, float>[]
				{
					new KeyValuePair<string, float>("Auto", 0),
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

			for(int i=0; i<apertureValues.Length; i++)
			{
				if(apertureValues[i].Value == options.Aperture)
				{
					cmbAperture.SelectedIndex = i;
					break;
				}
			}

			_isInitialized = true;
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
	}
}
