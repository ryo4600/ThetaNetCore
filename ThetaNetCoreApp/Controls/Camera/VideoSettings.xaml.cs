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
using ThetaNetCoreApp.Properties;
using static ThetaNetCore.Wifi.InfoResponse;

namespace ThetaNetCoreApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for VideoSettings.xaml
	/// </summary>
	public partial class VideoSettings : UserControl
	{
		#region Members and Properties
		private ThetaWifiConnect _theta = null;

		bool _isInitialized = false;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public VideoSettings()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Set Theta instance
		/// </summary>
		/// <param name="theta"></param>
		internal void SetTheta(ThetaWifiConnect theta)
		{
			_theta = theta;
		}

		/// <summary>
		/// Visibility changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			if (!(bool)e.NewValue)
				return;

			InitializeControls();

			Task.Factory.StartNew(new Action(async () =>
			{
				await UpdateControls();
			}));
		}

		/// <summary>
		/// Initialize contorls bases
		/// </summary>
		private void InitializeControls()
		{
			if (CameraSharedInfo.Instance.Info.ThetaModel >= THETA_MODEL.V)
			{
				cmbPreview.ItemsSource = new PreviewFormat[]
				{
					new PreviewFormat() { Width=1920, Height=960, Framerate=8 },
					new PreviewFormat() { Width=1024, Height=512, Framerate=30 },
					new PreviewFormat() { Width=1024, Height=512, Framerate=8 },
					new PreviewFormat() { Width=640, Height=320, Framerate=30 },
					new PreviewFormat() { Width=640, Height=320, Framerate=8 },
					new PreviewFormat() { Width=640, Height=320, Framerate=8 }
				};

				if (!_isInitialized)
				{
					SetLastSettings();
				}

			}
			else
			{
				cmbPreview.ItemsSource = new PreviewFormat[]
				{
					new PreviewFormat() { Width=640, Height=320, Framerate=10 }
				};
			}

			_isInitialized = true;
		}

		/// <summary>
		/// Apply last settings to camera <br />
		/// Because some settings (ex. preview format) are set back to defaults.
		/// </summary>
		private void SetLastSettings()
		{
			Settings settings = Settings.Default;
			if(settings.LastSelectedPreviewResolution >= 0 &&
				settings.LastSelectedPreviewResolution < cmbPreview.Items.Count)
			{
				cmbPreview.SelectedIndex = settings.LastSelectedPreviewResolution;
			}
		}

		/// <summary>
		/// Update controls
		/// </summary>
		/// <returns></returns>
		async private Task UpdateControls()
		{
			if (_theta == null)
				return;

			var optionsParam = new GetOptionsParam() { PreviewFormat = true };
			var options = await _theta.ThetaApi.GetOptionsAsync(optionsParam);

			await this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
			{
				_ignoreEvent = true;
				cmbPreview.SelectedItem = options.PreviewFormat;
				_ignoreEvent = false;
			}));
		}

		private bool _ignoreEvent = false;
		/// <summary>
		/// Selection of preview is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CmbPreview_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_ignoreEvent)
				return;

			if (_theta == null)
				return;

			var options = new OptionValues();
			options.PreviewFormat = cmbPreview.SelectedItem as PreviewFormat;
			Task.Factory.StartNew(new Action(async () =>
			{
				await _theta.ThetaApi.SetOptionsAsync(options);
				CameraSharedInfo.Instance.FireRestartPreview();
			}));
			Settings settings = Settings.Default;
			settings.LastSelectedPreviewResolution = cmbPreview.SelectedIndex;
			settings.Save();

		}

	}
}
