using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThetaCamera;
using ThetaCamera.Request;
using ThetaToGo.Helper;
using ThetaToGo.Info;
using ThetaToGo.Log;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaToGo.Ctrl
{
	public sealed partial class DeviceImageListCtrl : UserControl
	{
		#region Events
		/// <summary>
		/// Sends a message to show.
		/// </summary>
		public event Action<String> MessageRequested = null;
		#endregion

		#region Members and Properties

		private ThetaConnect _theta;

		private ObservableCollection<FileEntryGroup> _thumbnails;

		public ThetaConnect Theta
		{
			get { return _theta; }
			set { _theta = value; }
		}

		private ObservableCollection<FileEntryWrapper> _deleteList = new ObservableCollection<FileEntryWrapper>();

		private bool _cancelRequested = false;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceImageListCtrl()
		{
			this.InitializeComponent();
			_deleteArea.DataContext = _deleteList;
			_lstDelete.DataContext = _deleteList;

			this.Loaded += (o, e) =>
			{
				if (_thumbnails == null)
					ListImages();

				this.Foreground = new SolidColorBrush(MySettings.Instance.ForeColor);
				this.Background = new SolidColorBrush(MySettings.Instance.BackColor);
			};
		}

		/// <summary>
		/// Sends message to show.
		/// </summary>
		/// <param name="msg"></param>
		private void SendMessage(string msg)
		{
			if (MessageRequested != null)
				MessageRequested(msg);
		}

		/// <summary>
		/// Cancel all processing (downloading, etc...)
		/// </summary>
		internal void CancelAll()
		{
			_cancelRequested = true;
		}

		object _lockObj = null;
		private enum IMG_FILTER { ALL, NOT, DONE };
		const int NUM_ENTRY = 50;
		/// <summary>
		/// Load images in the device
		/// </summary>
		/// <param name="theta"></param>
		public void ListImages()
		{
			if (_theta == null)
				return;

			_deleteList.Clear();

			IMG_FILTER filter = IMG_FILTER.ALL;
			switch (_cmbFilter.SelectedIndex)
			{
				case 1:
					filter = IMG_FILTER.NOT;
					break;
				case 2:
					filter = IMG_FILTER.DONE;
					break;
			}

			Task.Factory.StartNew(new Action(async () =>
			{
				while (_lockObj != null)
				{
					_cancelRequested = true;
					await Task.Delay(100);
				}

				_lockObj = new object();

				SendMessage(MyResources.Resources.GetString("Msg_ListingThetaImages"));
				try
				{
					var entries = new List<FileEntryWrapper>();
					int i = 0;
					while (true)
					{
						var param = new ListFilesParam() { FileType = ThetaCamera.ThetaFileType.Image, EntryCount = NUM_ENTRY, StartPosition = entries.Count, Detail = false };
						var res = await _theta.ListFilesAsync(param);
						foreach (var anEntry in res.Entries)
						{
							entries.Add(new FileEntryWrapper() { Data = anEntry, EntryNo = i++ });
						}

						var progress = (int)Math.Round((entries.Count / (double)res.TotalEntries) * 100);
						SendMessage(String.Format(MyResources.Resources.GetString("Msg_LoadingFileEntries"), progress));

						if (entries.Count >= res.TotalEntries)
							break;
					}

					StorageFolder devFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AppDefs.DEVICE_IMAGE, CreationCollisionOption.OpenIfExists);
					StorageFolder thumbFolder = await devFolder.CreateFolderAsync(AppDefs.THUMBNAIL, CreationCollisionOption.OpenIfExists);

					Dictionary<String, StorageFile> localFiles = new Dictionary<string, StorageFile>();
					// List up all thumbnails which are already loaded.
					var query = thumbFolder.CreateFileQuery(CommonFileQuery.OrderByName);
					query.ApplyNewQueryOptions(new QueryOptions(CommonFileQuery.OrderByName, new List<string>() { ".jpg" }));
					var files = await query.GetFilesAsync();
					foreach (var file in files)
					{
						localFiles.Add(file.Name, file);
					}

					await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
					{
						var thetaFolder = GetImageFolder();
						_thumbnails = new ObservableCollection<FileEntryGroup>();
						_fileList.DataContext = _thumbnails;
						var groups = (from a in entries group a by a.SimpleDate).ToArray();
						foreach (var g in groups)
						{
							var downloadList = new List<FileEntryWrapper>();
							SendMessage(String.Format(MyResources.Resources.GetString("Msg_Preparing"), g.Key));
							var wrappers = new ObservableCollection<FileEntryWrapper>();

							foreach (var item in g)
							{
								if (_cancelRequested)
									return;

								// Check if the image is already downloaded.
								var file = await thetaFolder.TryGetItemAsync(item.Data.Name) as StorageFile;
								if (file != null) // && (await file.GetBasicPropertiesAsync()).Size == (ulong)item.Data.Size)
									item.DownloadState = 100;

								switch (filter)
								{
									case IMG_FILTER.DONE:
										if (item.DownloadState != 100)
										{
											if (localFiles.ContainsKey(item.Data.Name))
												localFiles.Remove(item.Data.Name);
											continue;
										}
										break;
									case IMG_FILTER.NOT:
										if (item.DownloadState == 100)
										{
											if (localFiles.ContainsKey(item.Data.Name))
												localFiles.Remove(item.Data.Name);
											continue;
										}
										break;
								}
								wrappers.Add(item);
								downloadList.Add(item);
							}

							if (wrappers.Count > 0)
							{
								_thumbnails.Add(new FileEntryGroup() { Title = g.Key, Files = wrappers });
								await DownloadThumbnails(downloadList, localFiles, thumbFolder);
								await Task.Delay(10);
							}
						}

						foreach (var anEntry in _thumbnails.ToArray())
						{
							if (anEntry.Files.Count == 0)
								_thumbnails.Remove(anEntry);
						}

						SendMessage("");

						// The rests are deleted from the device, so we can delete thumbnails too 
						foreach (StorageFile file in localFiles.Values)
							await file.DeleteAsync();
					});
				}
				catch (Exception ex)
				{
					SendMessage("");
				}
				finally
				{
					_lockObj = null;
				}
			}));
		}

		/// <summary>
		/// Filter selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListImages();
		}

		/// <summary>
		/// Download thumbnails if one does not exists on local environment.
		/// </summary>
		/// <param name="entries"></param>
		async private Task DownloadThumbnails(List<FileEntryWrapper> entries, Dictionary<String, StorageFile> localFiles, StorageFolder thumbFolder)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				if (_cancelRequested)
					return;

				FileEntryWrapper anEntry = entries[i];

				// Check if the thumbnail is downloaded.
				IRandomAccessStream fileStream = null;
				if (localFiles.ContainsKey(anEntry.Data.Name))
				{
					// File is already loaded.
					fileStream = await localFiles[anEntry.Data.Name].OpenAsync(FileAccessMode.Read);
					localFiles.Remove(anEntry.Data.Name);
				}
				else
				{
					SendMessage(String.Format(MyResources.Resources.GetString("Msg_LoadingThumbnail"), anEntry.Data.Name));
					// Load from the device
					var param = new ListFilesParam() { FileType = ThetaCamera.ThetaFileType.Image, EntryCount = 1, Detail = true, StartPosition = anEntry.EntryNo };
					try
					{
						var res = await _theta.ListFilesAsync(param);
						if (res.Entries.Length == 0)
							continue;
						var detailedEntry = res.Entries[0];

						using (var memStream = new MemoryStream())
						{
							await memStream.WriteAsync(detailedEntry.Thumbnail, 0, detailedEntry.Thumbnail.Length);

							var newFile = await thumbFolder.CreateFileAsync(detailedEntry.Name);
							using (Stream aStream = await newFile.OpenStreamForWriteAsync())
							{
								memStream.Seek(0, SeekOrigin.Begin);
								memStream.CopyTo(aStream);
							}
							fileStream = await newFile.OpenAsync(FileAccessMode.Read);
						}
					}
					catch (System.Net.WebException)
					{
						String msg = MyResources.LogMessages.GetString("ConnectionFailed");
						LogManager.Instance.AddOperationErrorLog(msg, null, this.Dispatcher);
						break;
					}
					catch (Exception ex)
					{
						continue;
					}
				}

				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.SetSource(fileStream);
				anEntry.ThumbImage = bitmapImage;
				await Task.Delay(1);
			}

		}

		#region Download image
		private int _numDataToDownload = 0;
		Queue<FileEntryWrapper> _downloadQueue = new Queue<FileEntryWrapper>();
		BackgroundWorker _worker = null;

		/// <summary>
		/// Download button is clicked
		/// </summary>
		private void DeviceImageCtrl_DownloadRequested(FileEntryWrapper wrapper)
		{
			if (wrapper == null)
				return;

			if (_worker == null)
			{
				_worker = new BackgroundWorker();
				_worker.WorkerSupportsCancellation = true;
				_worker.DoWork += download_DoWork;
				_worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
			}

			_numDataToDownload++;
			wrapper.DownloadState = 0;
			_downloadQueue.Enqueue(wrapper);
			ShowRemaining();
			if (_downloadQueue.Count == 1)
			{
				_worker.RunWorkerAsync();
			}
		}

		/// <summary>
		/// Common function to show download remaining.
		/// </summary>
		private void ShowRemaining()
		{
			Task.Factory.StartNew(new Action(async () =>
			{
				await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					if (_downloadQueue.Count > 0)
						_txtLoadProgress.Text = String.Format(MyResources.Resources.GetString("Msg_LoadRemaining"), _downloadQueue.Count, _numDataToDownload);
					else
						_txtLoadProgress.Text = "";
				});
			}));
		}

		/// <summary>
		/// Downloads are completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//_workerCompleted = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void download_DoWork(object sender, DoWorkEventArgs e)
		{
			StorageFolder thetaFolder = GetImageFolder();

			while (true)
			{
				if (_cancelRequested)
				{
					_downloadQueue.Clear();
					break;
				}
				var wrapper = _downloadQueue.Peek();
				try
				{
					// Get a read stream
					using (Stream stream = await _theta.GetImageAsync(wrapper.Data.FileUrl))
					{
						// Read one by one
						var size = wrapper.Data.Size;
						var newFile = await thetaFolder.CreateFileAsync(wrapper.Data.Name, CreationCollisionOption.ReplaceExisting);
						using (Stream aStream = await newFile.OpenStreamForWriteAsync())
						{
							int readSize = 1000;
							var readBuffer = new byte[readSize];
							int totalRead = 0;
							for (int i = 0; i < (int)Math.Ceiling(size / (double)readSize); i++)
							{
								var numRead = await stream.ReadAsync(readBuffer, 0, readSize);

								aStream.Write(readBuffer, 0, numRead);
								totalRead += numRead;
								await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
								{
									wrapper.DownloadState = (int)Math.Round(totalRead / (double)size * 100.0);
								});
							}

							_downloadQueue.Dequeue();
							MySettings.Instance.LocalImageChanged = true;

							if (_downloadQueue.Count == 0)
								break;
						}
						ShowRemaining();
					}
				}
				catch (ThetaException the)
				{
					LogManager.Instance.AddOperationErrorLog(MyResources.LogOperations.GetString("Download"), the, this.Dispatcher);
					break;
				}
				catch (System.Net.WebException)
				{
					String msg = MyResources.LogMessages.GetString("ConnectionFailed");
					LogManager.Instance.AddOperationErrorLog(msg, null, this.Dispatcher);
					break;
				}
				catch (Exception ex)
				{
					String msg = MyResources.LogMessages.GetString("ConnectionFailed");
					LogManager.Instance.AddOperationErrorLog(msg, ex, this.Dispatcher);
					break;
				}
			}
			_numDataToDownload = 0;
			ShowRemaining();
		}

		/// <summary>
		/// Get folder that images are stored.
		/// </summary>
		/// <returns></returns>
		private static StorageFolder GetImageFolder()
		{
			return MySettings.Instance.SaveFolder;
			//var picFolder = KnownFolders.PicturesLibrary;
			//StorageFolder thetaFolder = await picFolder.CreateFolderAsync(AppDefs.LOCAL_IMAGE, CreationCollisionOption.OpenIfExists);
			//return thetaFolder;
		}
		#endregion

		/// <summary>
		/// List item's download button is clicked
		/// </summary>
		private void DeviceImageCtrl_DeleteRequested(FileEntryWrapper wrapper)
		{
			_deleteList.Add(wrapper);

			var fileList = _fileList.DataContext as ObservableCollection<FileEntryGroup>;
			foreach (var aGroup in fileList)
			{
				if (aGroup.Files.Contains(wrapper))
				{
					aGroup.Files.Remove(wrapper);
					break;
				}
			}
		}


		/// <summary>
		/// Delete image
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDelete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Task.Factory.StartNew(new Action(async () =>
			{
				await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
				{
					foreach (var wrapper in _deleteList.ToArray())
					{
						try
						{
							await _theta.DeleteAsync(new String[] { wrapper.Data.FileUrl });
							_deleteList.Remove(wrapper);
						}
						catch (Exception ex)
						{
							LogManager.Instance.AddOperationErrorLog(MyResources.LogOperations.GetString("DeleteDeviceFile"), ex, this.Dispatcher);
						}
					}
					MySettings.Instance.LocalImageChanged = true;
				});
			}));
		}

		/// <summary>
		/// Cancel button in delete area is being clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			ListImages();
		}
	}
}
