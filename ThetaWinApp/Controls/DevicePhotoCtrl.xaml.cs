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

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// Control to view phots taken still in device
	/// </summary>
	public partial class DevicePhotoCtrl : UserControl
	{
		PhotoViewWnd _photoWnd = null;
		private ThetaWifiConnect _theta = null;

		public DevicePhotoCtrl()
		{
			InitializeComponent();
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

			UpdateDownloadButton();
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
		/// Text for Download path changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TxtFolder_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDownloadButton();
		}

		/// <summary>
		/// Update state of the "download" button
		/// </summary>
		private void UpdateDownloadButton()
		{
			btnDownload.IsEnabled = Directory.Exists(txtFolder.Text) && lstFiles.DataContext != null;
		}

		/// <summary>
		/// Refresh list button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
		{
			await ReloadAllFiles();
		}

		Queue<FileEntryWrapper> _getThumbnailQueue = new Queue<FileEntryWrapper>();

		/// <summary>
		/// Reload all files
		/// </summary>
		public async Task ReloadAllFiles(bool force=true)
		{
			if (!force && lstFiles.DataContext != null)
				return;

			var entries = new List<FileEntryWrapper>();
			// Get xx files in each iteration. EntryCount and StartPosition is important.
			while (true)
			{
				var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 50, StartPosition = entries.Count, Detail = false };
				var res = await _theta.ThetaApi.ListFilesAsync(param);
				foreach (var anEntry in res.Entries)
				{
					var wrapper = new FileEntryWrapper() { Data = anEntry, EntryNo = entries.Count };
					entries.Add(wrapper);
					_getThumbnailQueue.Enqueue(wrapper);
				}

				if (entries.Count >= res.TotalEntries)
					break;
			}

			var view = (CollectionView)CollectionViewSource.GetDefaultView(entries);
			var groupDesc = new PropertyGroupDescription("SimpleDate");
			view.GroupDescriptions.Add(groupDesc);

			await Task.Delay(1);

			groupDesc.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
			lstFiles.DataContext = entries;

			GetThumbnails();
		}

		/// <summary>
		/// Download thumbnails
		/// </summary>
		async private void GetThumbnails()
		{
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

							var newFile = new FileInfo( Path.Combine(thumbFolder.FullName, anEntry.Data.Name));
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
				localFiles[key].Delete();
		}

		/// <summary>
		/// Download button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnDownload_Click(object sender, RoutedEventArgs e)
		{
			//var anEntry = thumbnailBiew.DataContext as FileEntry;

			//// Get a read stream
			//using (Stream stream = await _theta.ThetaApi.GetImageAsync(anEntry.FileUrl))
			//{
			//	var path = System.IO.Path.Combine(txtFolder.Text, anEntry.Name);
			//	var size = anEntry.Size;
			//	var newFile = new FileInfo(path);
			//	using (var aStream = newFile.Open(FileMode.OpenOrCreate))
			//	{
			//		int readSize = 1000;
			//		var readBuffer = new byte[readSize];
			//		int totalRead = 0;
			//		for (int i = 0; i < (int)Math.Ceiling(size / (double)readSize); i++)
			//		{
			//			var numRead = await stream.ReadAsync(readBuffer, 0, readSize);

			//			aStream.Write(readBuffer, 0, numRead);
			//			totalRead += numRead;
			//		}
			//	}
			//}
		}

		/// <summary>
		/// Image is double clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeviceImageCtrl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
		/// Checked event for item in listview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void chkListItem_Checked(object sender, RoutedEventArgs e)
		{
			//var selItems = lstFiles.SelectedItems;
			//if (selItems.Count > 0)
			//{
			//	var newVal = ((ToggleButton)sender).IsChecked.Value;
			//	for (int i = 0; i < selItems.Count; i++)
			//		((FileEntryWrapper)selItems[i]).IsChecked = newVal;
			//}
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

			await ReloadAllFiles();
		}
	}
}
