using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq;

using ThetaNetCore;
using ThetaNetCore.Wifi;
using ThetaWinApp.Properties;
using ThetaWinApp.Info;
using ThetaWinApp.Resources;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Threading;

namespace ThetaWinApp.Controls.Camera
{
	public enum PHOTO_FILTER { ALL, NOT_DOWNLOADED, DOWNLOADED }
	/// <summary>
	/// Control to view phots taken still in device
	/// </summary>
	public partial class DevicePhotoCtrl : UserControl
	{
		PhotoViewWnd _photoWnd = null;
		private ThetaWifiConnect _theta = null;
		Queue<FileEntryWrapper> _getThumbnailQueue = new Queue<FileEntryWrapper>();
		List<FileEntryWrapper> _deviceImages = new List<FileEntryWrapper>();


		/// <summary>
		/// Constructor
		/// </summary>
		public DevicePhotoCtrl()
		{
			InitializeComponent();

			this.Loaded += (sender, e) =>
			{
				var settings = Settings.Default;
				var filters = new List<KeyValuePair<PHOTO_FILTER, string>>()
				{
					new KeyValuePair<PHOTO_FILTER, string>(PHOTO_FILTER.ALL, AppStrings.Item_PhotoAll),
					new KeyValuePair<PHOTO_FILTER, string>(PHOTO_FILTER.NOT_DOWNLOADED, AppStrings.Item_PhotoNotDownloaded),
					new KeyValuePair<PHOTO_FILTER, string>(PHOTO_FILTER.DOWNLOADED, AppStrings.Item_PhotoDownloaded)
				};
				cmbImageFilter.SelectedValuePath = "Key";
				cmbImageFilter.DisplayMemberPath = "Value";
				cmbImageFilter.DataContext = filters;

				cmbImageFilter.SelectedIndex = settings.LastSelectedDevPhotoFilter < 0 ? 1 : settings.LastSelectedDevPhotoFilter;
			};


		}

		/// <summary>
		/// Set Theta control instance
		/// </summary>
		/// <param name="theta"></param>
		public void SetTheta(ThetaWifiConnect theta)
		{
			_theta = theta;
		}

		/// <summary>
		/// Loaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			txtFolder.Text = settings.DownloadPath;
		}

		/// <summary>
		/// Unloaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
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
				if (!String.IsNullOrEmpty(settings.DownloadPath))
					dlg.SelectedPath = settings.DownloadPath;

				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					settings.DownloadPath = dlg.SelectedPath;
					settings.Save();

					txtFolder.Text = dlg.SelectedPath;
				}
			}
		}

		/// <summary>
		/// Edit mode checked/unchecked event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToggleEdit_Checked(object sender, RoutedEventArgs e)
		{
			lstFiles.SelectionMode = toggleEdit.IsChecked.Value ? SelectionMode.Extended : SelectionMode.Single;
		}

		/// <summary>
		/// Refresh list button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
		{
			await ReloadAllFilesAsync(true);
		}

		/// <summary>
		/// Selection of image filter has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void cmbImageFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Avoid to check onLoaded event
			if (this.Visibility != Visibility.Visible)
				return;

			if (cmbImageFilter.SelectedIndex < 0)
				return;

			await ReloadAllFilesAsync(false);

			var settings = Settings.Default;
			settings.LastSelectedDevPhotoFilter = cmbImageFilter.SelectedIndex;
			settings.Save();
		}

		/// <summary>
		/// Reload all files
		/// </summary>
		public async Task ReloadAllFilesAsync(bool force = true)
		{
			if (force || _deviceImages.Count == 0)
			{
				_deviceImages.Clear();

				while (true)
				{
					var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 50, StartPosition = _deviceImages.Count, Detail = false };
					var res = await _theta.ThetaApi.ListFilesAsync(param);
					foreach (var anEntry in res.Entries)
					{
						var wrapper = new FileEntryWrapper() { Data = anEntry, EntryNo = _deviceImages.Count };
						_deviceImages.Add(wrapper);
						_getThumbnailQueue.Enqueue(wrapper);
					}

					if (_deviceImages.Count >= res.TotalEntries)
						break;
				}
			}

			var di = new DirectoryInfo(txtFolder.Text);
			// Get list of thumbnails already downloaded
			var existingFiles = di.EnumerateFiles().Where(file => file.Extension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
								file.Extension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase)).ToList();

			var localFiles = new Dictionary<string, FileInfo>();
			foreach (var file in existingFiles)
				localFiles.Add(file.Name, file);

			foreach(var anEntry in _deviceImages)
			{
				if (localFiles.ContainsKey(anEntry.Data.Name))
					anEntry.DownloadStatus = DOWNLOAD_STATUS.DOWNLOADED;
			}

			IEnumerable<FileEntryWrapper> filteredEntries = null;

			switch(cmbImageFilter.SelectedValue)
			{
				case PHOTO_FILTER.DOWNLOADED:
					filteredEntries = from anEntry in _deviceImages where anEntry.DownloadStatus == DOWNLOAD_STATUS.DOWNLOADED select anEntry;
					break;
				case PHOTO_FILTER.NOT_DOWNLOADED:
					filteredEntries = from anEntry in _deviceImages where anEntry.DownloadStatus != DOWNLOAD_STATUS.DOWNLOADED select anEntry;
					break;
				case PHOTO_FILTER.ALL:
				default:
					filteredEntries = _deviceImages;
					break;
			}

			var view = (CollectionView)CollectionViewSource.GetDefaultView(filteredEntries);
			var groupDesc = new PropertyGroupDescription("SimpleDate");
			view.GroupDescriptions.Add(groupDesc);

			groupDesc.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
			lstFiles.DataContext = filteredEntries;

			GetThumbnails();
		}

		/// <summary>
		/// Download thumbnails
		/// </summary>
		async private void GetThumbnails()
		{
			if (_getThumbnailQueue.Count == 0)
				return;

			var thumbFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ThetaImage", "thumbnails"));
			thumbFolder.Create();

			// Get list of thumbnails already downloaded
			var existingFiles = thumbFolder.EnumerateFiles().Where(file => file.Extension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
								file.Extension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase)).ToList();

			var localFiles = new Dictionary<string, FileInfo>();
			foreach (var file in existingFiles)
				localFiles.Add(file.Name, file);

			// For each entry...
			while (_getThumbnailQueue.Count > 0)
			{
				var anEntry = _getThumbnailQueue.Dequeue();
				if (localFiles.ContainsKey(anEntry.Data.Name))
				{
					// if the thumbnail is already downloaded, use that .
					anEntry.LocalThumbFile = localFiles[anEntry.Data.Name].FullName;
					localFiles.Remove(anEntry.Data.Name);

				}
				else
				{
					// Otherwise get it from camera
					var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 1, Detail = true, StartPosition = anEntry.EntryNo };
					try
					{
						var res = await _theta.ThetaApi.ListFilesAsync(param);
						if (res.Entries.Length == 0)
							continue;
						var result = res.Entries[0];

						using (var memStream = new MemoryStream())
						{
							await memStream.WriteAsync(result.ThumbnailData, 0, result.ThumbnailData.Length);

							var newFile = new FileInfo(Path.Combine(thumbFolder.FullName, anEntry.Data.Name));
							using (Stream aStream = newFile.Open(FileMode.CreateNew, FileAccess.Write))
							{
								memStream.Seek(0, SeekOrigin.Begin);
								memStream.CopyTo(aStream);
							}

							anEntry.LocalThumbFile = newFile.FullName;
						}
					}
					catch (System.Net.WebException)
					{
						break;
					}
					catch (Exception ex)
					{
						continue;
					}
				}
				await Task.Delay(1);
			}

			// Delete files that are no longer exist inside the camera
			foreach (var key in localFiles.Keys)
			{
				try
				{
					localFiles[key].Delete();
				}
				catch (IOException)
				{
					// This happens when we delete pictures.
					// It is too soon to delete thumbnails. Safe to delete next time.
				}
			}
		}

		/// <summary>
		/// Image is double clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PhotoCard_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var anEntry = ((FrameworkElement)sender).DataContext as FileEntryWrapper;
			ShowPhoto(anEntry.Data);
		}

		/// <summary>
		/// Show selected image in the phot window
		/// </summary>
		/// <param name="anEntry"></param>
		private void ShowPhoto(FileEntry anEntry)
		{
			if (anEntry == null)
			{
				return;
			}

			var img = new BitmapImage();
			img.BeginInit();
			img.CacheOption = BitmapCacheOption.OnLoad;
			img.UriSource = new Uri(anEntry.FileUrl);
			img.EndInit();

			if (_photoWnd == null)
			{
				CreatePhotoViewWindow();
			}

			_photoWnd.SetImage(img);
			_photoWnd.Visibility = Visibility.Visible;
		}

		/// <summary>
		/// Create photo window
		/// </summary>
		private void CreatePhotoViewWindow()
		{
			_photoWnd = new PhotoViewWnd();
			_photoWnd.Owner = App.Current.MainWindow;
			_photoWnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			_photoWnd.SaveWindowPosition = true;

			_photoWnd.NextPhotoRequested += () =>
			{
				var items = lstFiles.ItemsSource as IEnumerable<FileEntryWrapper>;
				var idx = lstFiles.SelectedIndex;
				if (++idx < items.Count())
				{
					lstFiles.SelectedIndex = idx;
					ShowPhoto(((FileEntryWrapper)lstFiles.SelectedItem).Data);
				}
			};

			_photoWnd.PrevPhotoRequested += () =>
			{
				var items = lstFiles.ItemsSource as IEnumerable<FileEntryWrapper>;
				var idx = lstFiles.SelectedIndex;
				if (--idx >= 0)
				{
					lstFiles.SelectedIndex = idx;
					ShowPhoto(((FileEntryWrapper)lstFiles.SelectedItem).Data);
				}
			};
		}

		/// <summary>
		/// Visibility changed event. Hide photo window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue)
			{
				if (_photoWnd != null && _photoWnd.Visibility == Visibility.Visible)
					_photoWnd.Visibility = Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Delete button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			var viewSource = lstFiles.ItemsSource as IEnumerable<FileEntryWrapper>;
			var items = (from item in viewSource where item.IsChecked select item).ToArray();

			if (items.Length == 0)
			{
				MessageBox.Show(AppStrings.Msg_ChooseFileToDelete, AppStrings.Title_ConfirmDelete);
				return;
			}

			if (MessageBox.Show(String.Format(AppStrings.Msg_ConfirmToDelete, items.Length), AppStrings.Title_ConfirmDelete, MessageBoxButton.YesNo) == MessageBoxResult.No)
			{
				return;
			}

			for (int i = 0; i < items.Length; i++)
			{
				await _theta.ThetaApi.DeleteAsync(new string[] { items[i].Data.FileUrl });
			}

			await ReloadAllFilesAsync();
		}

		bool _isDownloading = false;
		List<FileEntryWrapper> _downloadQueue = new List<FileEntryWrapper>();
		BackgroundWorker _downloadWorkder = null;
		/// <summary>
		/// Download request from one of images.
		/// </summary>
		/// <param name="entry"></param>
		private void PhotoCard_DownloadRequested(FileEntryWrapper entry)
		{
			if(txtFolder.Text == "")
			{
				MessageBox.Show(AppStrings.Err_DownloadFolder, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			entry.DownloadStatus = DOWNLOAD_STATUS.WAINTING;
			_downloadQueue.Add(entry);
			if(_downloadWorkder == null)
			{
				_downloadWorkder = new BackgroundWorker();
				_downloadWorkder.WorkerSupportsCancellation = true;
				_downloadWorkder.DoWork += _downloadWorkder_DoWork;
			}

			var parameters = new object[] { txtFolder.Text };
			//if (!_downloadWorkder.IsBusy)
			if(!_isDownloading)
				_downloadWorkder.RunWorkerAsync(parameters);
		}

		/// <summary>
		/// Download cancel request from one of images 
		/// </summary>
		/// <param name="entry"></param>
		private void PhotoCard_CancelRequested(FileEntryWrapper entry)
		{
			entry.DownloadStatus = DOWNLOAD_STATUS.NOT_DOWNLOADED;
			_downloadQueue.Remove(entry);
		}

		/// <summary>
		/// Process downloading in background
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void _downloadWorkder_DoWork(object sender, DoWorkEventArgs e)
		{
			var args = (object[])e.Argument;
			var savePath = (string)args[0];

			_isDownloading = true;
			while (_downloadQueue.Count > 0)
			{
				var anEntry = _downloadQueue[0];

				// Get a read stream
				using (Stream stream = await _theta.ThetaApi.GetImageAsync(anEntry.Data.FileUrl))
				{
					var fileName = System.IO.Path.Combine(savePath, anEntry.Data.Name);
					var size = anEntry.Data.Size;
					var newFile = new FileInfo(fileName);
					using (var aStream = newFile.Open(FileMode.OpenOrCreate))
					{
						int readSize = 1000;
						var readBuffer = new byte[readSize];
						int totalRead = 0;
						for (int i = 0; i < (int)Math.Ceiling(size / (double)readSize); i++)
						{
							var numRead = await stream.ReadAsync(readBuffer, 0, readSize);

							aStream.Write(readBuffer, 0, numRead);
							totalRead += numRead;
							anEntry.DownloadProgress = (int)((double)totalRead / size * 100);
						}
					}
				}

				anEntry.DownloadProgress = 100;

				_downloadQueue.Remove(anEntry);
			}
			_isDownloading = false;

			await this.ReloadAllFilesAsync();
		}

	}
}
