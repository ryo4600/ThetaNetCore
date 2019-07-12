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
using ThetaWinApp.Properties;

namespace ThetaWinApp.Controls.Others
{
	/// <summary>
	/// Interaction logic for SettingsCtrl.xaml
	/// </summary>
	public partial class SettingsCtrl : UserControl
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SettingsCtrl()
		{
			InitializeComponent();
			this.Loaded += SettingsCtrl_Loaded;
		}

		/// <summary>
		/// Loaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SettingsCtrl_Loaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			txtDownloadFolder.Text = settings.DownloadPath;
			txtPCFolder.Text = settings.PhotoPath;
		}

		/// <summary>
		/// "..." for download path is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDownloadFolder_Click(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			var path = settings.DownloadPath;
			if (ShowFolderSelection(ref path))
			{
				txtDownloadFolder.Text = path;
				settings.DownloadPath = path;
				settings.Save();
			}
		}

		/// <summary>
		/// "..." for PC view path is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnPCFolder_Click(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			var path = settings.PhotoPath;
			if (ShowFolderSelection(ref path))
			{
				txtPCFolder.Text = path;
				settings.PhotoPath = path;
				settings.Save();
			}
		}

		/// <summary>
		/// "Open" folder dialog 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private bool ShowFolderSelection(ref String path)
		{
			using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (!String.IsNullOrEmpty(path))
					dlg.SelectedPath = path;

				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					path = dlg.SelectedPath;
					return true;
				}
				return false;
			}
		}

	}
}
