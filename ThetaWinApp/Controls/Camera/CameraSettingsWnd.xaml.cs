using MahApps.Metro.Controls;
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
using System.Windows.Shapes;
using ThetaNetCore.Wifi;

namespace ThetaWinApp.Controls.Camera
{
	/// <summary>
	/// Interaction logic for CameraSettingsWnd.xaml
	/// </summary>
	public partial class CameraSettingsWnd : MetroWindow
	{
		#region Members and Properties
		private ThetaWifiConnect _theta = null;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public CameraSettingsWnd()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Set instance of theta
		/// </summary>
		/// <param name="theta"></param>
		public void SetTheta(ThetaWifiConnect theta)
		{
			_theta = theta;

			photoSettings.SetTheta(theta);
			imageSettings.SetTheta(theta);
			videoSettings.SetTheta(theta);
		}

		/// <summary>
		/// Prevent from deleting window. Hide instead.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.Visibility = Visibility.Collapsed;
			e.Cancel = true;
		}
	}
}
