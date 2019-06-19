using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ThetaNetCore;
using ThetaNetCore.Wifi;
using ThetaWinApp.Properties;

namespace ThetaWinApp.Controls
{
	/// <summary>
	/// Control to view phots taken still in device
	/// </summary>
	public partial class CameraPhotoViewCtrl : UserControl
	{
		private ThetaWifiConnect _theta = null;

		public CameraPhotoViewCtrl()
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
			txtSavePath.Text = settings.SavePath;

			UpdateDownloadButton();
		}

		/// <summary>
		/// Unloaded event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Default;
			settings.SavePath = txtSavePath.Text;
			settings.Save();
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
		public async Task ReloadAllFiles(bool force=true)
		{
			if (!force && lstFiles.DataContext != null)
				return;

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
				var path = System.IO.Path.Combine(txtSavePath.Text, anEntry.Name);
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

	}
}
