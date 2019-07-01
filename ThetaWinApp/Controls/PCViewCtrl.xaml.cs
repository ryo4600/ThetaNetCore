using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ThetaWinApp.Info;
using ThetaWinApp.Properties;
using ThetaWinApp.Resources;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// User control to list files in PC and view them
	/// </summary>
	public partial class PCViewCtrl : UserControl
	{
		PhotoViewWnd _photoWnd = null;

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

		List<ImageFileWrapper> _imageFiles = new List<ImageFileWrapper>();
		/// <summary>
		/// Find image files and set to the list
		/// </summary>
		/// <param name="selectedPath"></param>
		async private void SetImageFiles(string selectedPath)
		{
			pnlLoading.Visibility = Visibility.Visible;

			_imageFiles.Clear();
			DirectoryInfo di = new DirectoryInfo(selectedPath);
			if (di.Exists)
			{
				var files = di.EnumerateFiles().Where(file => file.Extension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
								file.Extension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase)).OrderByDescending(file => file.CreationTime);
				var numFiles = files.Count();
				if (files != null && numFiles > 0)
				{
					var counter = 1;
					foreach (var aFile in files)
					{
						_imageFiles.Add(new ImageFileWrapper() { File = aFile });
						prgLoading.Value = (double)counter / numFiles * 100.0;
						txtLoaing.Text = String.Format(AppStrings.Msg_FileLoading, counter++, numFiles);
						await Task.Delay(1);
					}
				}
			}

			var years = (from img in _imageFiles orderby img.DateTaken.Year descending select img.DateTaken.Year).Distinct();
//			cmbFilterYear.DisplayMemberPath = "Name";
			cmbFilterYear.ItemsSource = years;

			if (years.Count() > 0)
				cmbFilterYear.SelectedIndex = 0;
			pnlLoading.Visibility = Visibility.Collapsed;

		}

		/// <summary>
		/// Edit mode checked/unchecked event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToggleEdit_Checked(object sender, RoutedEventArgs e)
		{
			lstPcFiles.SelectionMode = toggleEdit.IsChecked.Value ? SelectionMode.Extended : SelectionMode.Single;
		}

		/// <summary>
		/// Image is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LocalImageCtrl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var imgData = ((FrameworkElement)sender).DataContext as ImageFileWrapper;
            ShowPhoto(imgData);
        }

        /// <summary>
        /// Show selected image in the phot window
        /// </summary>
        /// <param name="imgData"></param>
        private void ShowPhoto(ImageFileWrapper imgData)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(imgData.File.FullName);
            img.EndInit();
            img.Freeze();


            if (_photoWnd == null)
            {
                CreatePhotWindow();

            }

            _photoWnd.SetImage(img);
            _photoWnd.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Create photo window
        /// </summary>
        private void CreatePhotWindow()
        {
            _photoWnd = new PhotoViewWnd();
            _photoWnd.Owner = App.Current.MainWindow;
            _photoWnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _photoWnd.SaveWindowPosition = true;

            _photoWnd.NextPhotoRequested += () =>
            {
                var items = lstPcFiles.ItemsSource as IEnumerable<ImageFileWrapper>;
                var idx = lstPcFiles.SelectedIndex;
                if (++idx < items.Count())
                {
                    lstPcFiles.SelectedIndex = idx;
                    ShowPhoto(lstPcFiles.SelectedItem as ImageFileWrapper);
                }
            };

            _photoWnd.PrevPhotoRequested += () =>
            {
                var items = lstPcFiles.ItemsSource as IEnumerable<ImageFileWrapper>;
                var idx = lstPcFiles.SelectedIndex;
                if (--idx > 0)
                {
                    lstPcFiles.SelectedIndex = idx;
                    ShowPhoto(lstPcFiles.SelectedItem as ImageFileWrapper);
                }

            };
        }

        /// <summary>
        /// Year filter is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbFilterYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(cmbFilterYear.SelectedItem != null)
				SetFilteredImages((int)cmbFilterYear.SelectedItem);
		}

		/// <summary>
		/// Set images filtered by year
		/// </summary>
		/// <param name="year"></param>
		async private void SetFilteredImages(int year)
		{
			pnlLoading.Visibility = Visibility.Visible;
			await Task.Delay(1);

			var filteredImgs = from img in _imageFiles where img.DateTaken.Year == year select img;

			var view = (CollectionView)CollectionViewSource.GetDefaultView(filteredImgs);
			var groupDesc = new PropertyGroupDescription("DateString");
			view.GroupDescriptions.Add(groupDesc);

			txtLoaing.Text = AppStrings.Msg_FileSorting;
			await Task.Delay(1);

			groupDesc.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
			lstPcFiles.DataContext = filteredImgs;

			pnlLoading.Visibility = Visibility.Collapsed;
		}
	}
}
