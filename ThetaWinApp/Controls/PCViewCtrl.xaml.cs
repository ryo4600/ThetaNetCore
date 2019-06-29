using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using ThetaWinApp.Info;
using ThetaWinApp.Properties;
using static ThetaWinApp.Controls.LocalImageListCtrl;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// User control to list files in PC and view them
	/// </summary>
	public partial class PCViewCtrl : UserControl
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public PCViewCtrl()
		{
			InitializeComponent();

			this.Loaded += PCViewCtrl_Loaded;
		}

		/// <summary>
		/// Loaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PCViewCtrl_Loaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			if (!String.IsNullOrEmpty(settings.ImageBrowsePath))
				txtFolder.Text = settings.ImageBrowsePath;
		}

		/// <summary>
		/// "Open" folder button is clicked 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFolder_Click(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (!String.IsNullOrEmpty(settings.ImageBrowsePath))
					dlg.SelectedPath = settings.ImageBrowsePath;

				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					settings.ImageBrowsePath = dlg.SelectedPath;
					settings.Save();

					txtFolder.Text = dlg.SelectedPath;
				}
			}
		}

		/// <summary>
		/// Text of folder box has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TxtFolder_TextChanged(object sender, TextChangedEventArgs e)
		{
			var path = txtFolder.Text;
			if (Directory.Exists(path))
			{
				SetImageFiles(path);
			}
		}

		/// <summary>
		/// Find image files and set to the list
		/// </summary>
		/// <param name="selectedPath"></param>
		private void SetImageFiles(string selectedPath)
		{
			Task.Factory.StartNew(new Action(() =>
			{
				var foundFiles = new List<ImageFileWrapper>();
				DirectoryInfo di = new DirectoryInfo(selectedPath);
				if (di.Exists)
				{
					var files = di.EnumerateFiles().Where(file => file.Extension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
									file.Extension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase));
					if (files != null && files.Count() > 0)
					{
						foreach (var aFile in files)
							foundFiles.Add(new ImageFileWrapper() { File = aFile });
					}
				}
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
				{
					var view = (CollectionView)CollectionViewSource.GetDefaultView(foundFiles);
					var groupDesc = new PropertyGroupDescription("DateString");
					groupDesc.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
					view.GroupDescriptions.Add(groupDesc);
					lstPcFiles.DataContext = foundFiles;
				}));
			}));
		}

		/// <summary>
		/// File is selected in the PC file list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LstPcFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var aFile = lstPcFiles.SelectedItem as FileInfo;
			if (aFile == null)
				return;

			var Image = new BitmapImage();
			Image.BeginInit();
			Image.CacheOption = BitmapCacheOption.OnLoad;
			Image.UriSource = new Uri(aFile.FullName);
			Image.EndInit();
			Image.Freeze();

			viewSphere.Source = Image;
		}

	}
}
