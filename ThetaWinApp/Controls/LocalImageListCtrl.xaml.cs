using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ThetaWinApp.Info;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaWinApp.Controls
{
	public sealed partial class LocalImageListCtrl : UserControl
	{
		#region Event
		/// <summary>
		/// Sends a message to show.
		/// </summary>
		public event Action<String> MessageRequested = null;

		public event Action<object> ViewImageRequested = null;
		#endregion

		#region Members and Properties
		/// <summary>
		/// Image file folder
		/// </summary>
		public DirectoryInfo ImageFolder { get; set; }
		//public StorageFolder ImageFolder { get; set; }

		public String MruToken { get; set; }

		//private SortedDictionary<String, ObservableCollection<ImageFileWrapper>> _imageList;

		private ObservableCollection<ImageFileWrapper> _deleteList = new ObservableCollection<ImageFileWrapper>();
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public LocalImageListCtrl()
		{
			this.InitializeComponent();
			_deleteArea.DataContext = _deleteList;
			_lstDelete.DataContext = _deleteList;
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
		/// Load images in the device
		/// </summary>
		/// <param name="theta"></param>
		public void ListImages()
		{
			_deleteList.Clear();

			Task.Factory.StartNew(new Action(async () =>
			{
				await this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action( () =>
				{
					SendMessage("FindingImageFiles");
				}));
				var entries = new List<ImageFileWrapper>();
				var files = ImageFolder.EnumerateFiles("*.jpg");
				foreach (var aFile in files)
				{
					entries.Add(new ImageFileWrapper() { File = aFile });
				}

				await this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async() =>
				{
					//_imageList = new SortedDictionary<string, ObservableCollection<Info.ImageFileWrapper>>(new ReverseComparer<String>(StringComparer.CurrentCultureIgnoreCase));

					//SendMessage(MyResources.Resources.GetString("Msg_GroupingAndSorting"));
					//await Task.Delay(10);
					//CollectionViewSource viewSource = new CollectionViewSource() { IsSourceGrouped = true, ItemsPath = new PropertyPath("Value") };
					//_fileList.DataContext = viewSource;
					//var groups = (from a in entries orderby a.TimeTaken descending group a by a.CompareDate).ToArray();
					//foreach (var g in groups)
					//{
					//	var wrappers = new ObservableCollection<ImageFileWrapper>();
					//	foreach (var item in g)
					//	{
					//		wrappers.Add(item);
					//	}
					//	_imageList.Add(g.Key, wrappers);
					//	viewSource.Source = _imageList;
					//}

					var imageList = new ObservableCollection<FileDateGroup>();
					_fileList.DataContext = imageList;
					SendMessage("GroupingAndSorting");

					var groups = (from a in entries orderby a.DateTaken descending group a by a.DateTaken).ToArray();
					foreach (var g in groups)
					{
						var wrappers = new ObservableCollection<ImageFileWrapper>();
						imageList.Add(new FileDateGroup() { Title = g.Key.ToShortTimeString(), Files = wrappers });
						foreach (var item in g)
						{
							wrappers.Add(item);
							await Task.Delay(10);
						}
					}

					SendMessage("LoadingThumbnails");
					SendMessage("");

				}));
			}));
		}

		/// <summary>
		/// Loaded event for every item in the list
		/// </summary>
		/// <param name="data"></param>
		async private void LocalImageCtrl_Loaded(ImageFileWrapper data)
		{
			if (data == null || data.ThumbImage != null)
				return;
			await DownloadThumbnails(data);
		}

		/// <summary>
		/// Download thumbnails if one does not exists on local environment.
		/// </summary>
		/// <param name="anEntry"></param>
		async private Task DownloadThumbnails(ImageFileWrapper anEntry)
		{
			//var thumb = await anEntry.File.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
			//if (thumb != null)
			//{
			//	BitmapImage bitmapImage = new BitmapImage();
			//	bitmapImage.SetSource(thumb);
			//	anEntry.ThumbImage = bitmapImage;
			//}
			//else
			//{
			//	BitmapImage bitmapImage = new BitmapImage();
			//	bitmapImage.SetSource(await anEntry.File.OpenAsync(FileAccessMode.Read));
			//	anEntry.ThumbImage = bitmapImage;
			//}
		}

		/// <summary>
		/// List item's download button is clicked
		/// </summary>
		private void DeviceImageCtrl_DeleteRequested(ImageFileWrapper wrapper)
		{
			_deleteList.Add(wrapper);
			var fileList = _fileList.DataContext as ObservableCollection<FileDateGroup>;
			foreach (var aGroup in fileList)
			{
				if (aGroup.Files.Contains(wrapper))
				{
					aGroup.Files.Remove(wrapper);
					break;
				}
			}
			//foreach (var g in _imageList)
			//{
			//	if (g.Value.Contains(wrapper))
			//	{
			//		g.Value.Remove(wrapper);
			//		return;
			//	}
			//}
		}


		/// <summary>
		/// Delete image
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			foreach (var wrapper in _deleteList.ToArray())
			{
				try
				{
					wrapper.File.Delete();
					_deleteList.Remove(wrapper);
				}
				catch (Exception ex)
				{
					//LogManager.Instance.AddOperationErrorLog(MyResources.LogOperations.GetString("DeleteLocalFile"), ex, this.Dispatcher);
				}
			}
		}

		/// <summary>
		/// Cancel button in delete area is being clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			ListImages();
		}


		///// <summary>
		///// Get folder that images are stored.
		///// </summary>
		///// <returns></returns>
		//private static async Task<StorageFolder> GetImageFolder()
		//{
		//	var picFolder = KnownFolders.PicturesLibrary;
		//	StorageFolder thetaFolder = await picFolder.CreateFolderAsync(AppDefs.LOCAL_IMAGE, CreationCollisionOption.OpenIfExists);
		//	return thetaFolder;
		//}

		/// <summary>
		/// View image requested.
		/// </summary>
		/// <param name="wrapper"></param>
		private void LocalImageCtrl_ViewImageRequested(ImageFileWrapper wrapper)
		{
			var fileList = _fileList.DataContext as ObservableCollection<FileDateGroup>;
			RequestShowImage(wrapper, fileList);
		}

		private void RequestShowImage(ImageFileWrapper wrapper, ObservableCollection<FileDateGroup> fileList)
		{
			var images = new List<ImageFileWrapper>();
			foreach (var aGroup in fileList)
			{
				images.AddRange(aGroup.Files.ToArray());
			}
			//foreach (var collection in _imageList.Values)
			//{
			//	images.AddRange(collection.ToList());
			//}

			object[] parameters = new object[] {
				images,
				wrapper
			};

			if (ViewImageRequested != null)
				ViewImageRequested(parameters);
		}

		public class FileDateGroup
		{
			public String Title { get; set; }
			public ObservableCollection<ImageFileWrapper> Files { get; set; }
		}

	}
}
