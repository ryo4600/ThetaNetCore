﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ThetaNetCore;
using ThetaNetCore.Util;
using ThetaNetCore.Wifi;
using ThetaWinApp.Properties;

namespace ThetaWinApp
{
	/// <summary>
	/// Main window
	/// </summary>
	public partial class MainWindow : Window
	{
		ThetaWifiConnect _theta = new ThetaWifiConnect();

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			ctrlDeviceConnect.ConnectionEstablished += () =>
			{
				imgPreview.IsEnabled = true;
			};
			ctrlDeviceConnect.Theta = _theta;
		}

		/// <summary>
		/// General Event Handler<br/>
		/// Windows loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			if (settings.FormWidth > 0)
			{
				this.Width = settings.FormWidth;
				this.Height = settings.FormHeight;
			}

			txtSavePath.Text = settings.SavePath;

			_theta.ImageReady += ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted += ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed += ThetaApi_OnTakePictureFailed;

			UpdateDownloadButton();
		}

		/// <summary>
		/// General Event Handler <br />
		/// Window is closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var settings = Settings.Default;
			if (this.WindowState == WindowState.Normal)
			{
				settings.FormWidth = this.Width;
				settings.FormHeight = this.Height;
			}
			settings.SavePath = txtSavePath.Text;
			settings.Save();

			_theta.ImageReady -= ThetaApi_ImageReady;
			_theta.OnTakePictureCompleted -= ThetaApi_OnTakePictureCompleted;
			_theta.OnTakePictureFailed -= ThetaApi_OnTakePictureFailed;

		}

		/// <summary>
		/// Tab selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void tabSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selItem = ((TabControl)sender).SelectedItem;
			if (selItem == tabPreview)
				TogglePreview(true);
			else if (selItem == tabFiles)
			{
				if (lstFiles.DataContext == null)
					await ReloadAllFiles();
			}
			else
				TogglePreview(false);
		}

		/// <summary>
		/// Preview check state is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void chkPreview_CheckedChanged(object sender, RoutedEventArgs e)
		{
			TogglePreview(chkPreview.IsChecked.Value);
		}

		/// <summary>
		/// Start / stop preview
		/// </summary>
		/// <param name="startPreview"></param>
		public void TogglePreview(bool startPreview)
		{
			// prevent an event before initialization
			if (!tabPreview.IsEnabled)
				return;

			if (chkPreview.IsChecked.Value && startPreview)
			{
				_theta.StartLivePreview();
			}
			else if (_theta.IsPreviewing)
			{
				_theta.StopLivePreview();

			}
		}

		/// <summary>
		/// Preview image is ready
		/// </summary>
		/// <param name="imgByteArray"></param>
		private void ThetaApi_ImageReady(byte[] imgByteArray)
		{
			// JPEG format data
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				try
				{
					var source = LoadImage(imgByteArray);
					imgPreview.Source = source;
				}
				catch (Exception ex)
				{
//					HandleException(ex);
				}

			}));
		}

		/// <summary>
		/// Load bitmap image from byte array
		/// </summary>
		/// <param name="imageData"></param>
		/// <returns></returns>
		private static BitmapImage LoadImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0)
				return null;

			var image = new BitmapImage();
			using (var mem = new MemoryStream(imageData))
			{
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();
			return image;
		}

		/// <summary>
		/// Take picture button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void BtnTakePicture_Click(object sender, RoutedEventArgs e)
		{
			pnlPrgress.Visibility = Visibility.Visible;
			await _theta.TakePictureAsync();
			txtProgress.Text = "Processing ... ";
		}

		/// <summary>
		/// Take picture completed
		/// </summary>
		/// <param name="fileName"></param>
		private void ThetaApi_OnTakePictureCompleted(string fileName)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				pnlPrgress.Visibility = Visibility.Collapsed;
			}));
		}

		/// <summary>
		/// Take picture failed
		/// </summary>
		/// <param name="ex"></param>
		private void ThetaApi_OnTakePictureFailed(Exception ex)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
			{
				pnlPrgress.Visibility = Visibility.Collapsed;
			}));
		}

		/// <summary>
		/// Refresh list button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnRefreshList_Click(object sender, RoutedEventArgs e)
		{
			await ReloadAllFiles();
		}

		/// <summary>
		/// Reload all files
		/// </summary>
		private async Task ReloadAllFiles()
		{
			var entries = new List<FileEntry>();
			// Get xx files in each iteration. EntryCount and StartPosition is important.
			while (true)
			{
				var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 50, StartPosition = entries.Count, Detail = false };
				var res = await _theta.ThetaApi.ListFilesAsync(param);
				foreach (var anEntry in res.Entries)
				{
					entries.Add(anEntry);
				}

				if (entries.Count >= res.TotalEntries)
					break;
			}

			lstFiles.DataContext = entries;

		}

		/// <summary>
		/// Selection of file list is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void lstFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var anEntry = lstFiles.SelectedItem as FileEntry;
			if (anEntry != null && anEntry.Thumbnail == null)
			{
				var param = new ListFilesParam() { FileType = ThetaFileType.Image, EntryCount = 1, Detail = true, StartPosition = lstFiles.SelectedIndex };
				try
				{
					var res = await _theta.ThetaApi.ListFilesAsync(param);
					if (res.Entries.Length != 0)
						anEntry = res.Entries[0];
				}
				catch
				{
				}
			}

			imgPictureView.DataContext = anEntry;
			UpdateDownloadButton();
		}


		/// <summary>
		/// Choose save path (...) is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnPath_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			if (!String.IsNullOrWhiteSpace(txtSavePath.Text))
				dlg.SelectedPath = txtSavePath.Text;

			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				txtSavePath.Text = dlg.SelectedPath;
			}

		}

		/// <summary>
		/// Save path text changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TxtSavePath_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDownloadButton();
		}

		/// <summary>
		/// Update state of the "download" button
		/// </summary>
		private void UpdateDownloadButton()
		{
			btnDownload.IsEnabled = Directory.Exists(txtSavePath.Text) && imgPictureView.DataContext != null;
		}

		/// <summary>
		/// Download button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnDownload_Click(object sender, RoutedEventArgs e)
		{
			var anEntry = imgPictureView.DataContext as FileEntry;

			// Get a read stream
			using (Stream stream = await _theta.ThetaApi.GetImageAsync(anEntry.FileUrl))
			{
				var path = Path.Combine(txtSavePath.Text, anEntry.Name);
				var size = anEntry.Size;
				var newFile = new FileInfo(path);
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
					}
				}
			}
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

					SetImageFiles(dlg.SelectedPath);
				}
			}
		}

		/// <summary>
		/// Find image files and set to the list
		/// </summary>
		/// <param name="selectedPath"></param>
		private void SetImageFiles(string selectedPath)
		{
			List<FileInfo> foundFiles = new List<FileInfo>();
			DirectoryInfo di = new DirectoryInfo(selectedPath);
			if (di.Exists)
			{
				var files = di.EnumerateFiles().Where(file => file.Extension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
								file.Extension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase));
				if (files != null && files.Count() > 0)
				{
					foundFiles.AddRange(files.ToList());
				}
			}

			lstPcFiles.DataContext = foundFiles;
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