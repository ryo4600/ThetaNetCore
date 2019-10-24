using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThetaNetCoreApp.Controls
{
	/// <summary>
	/// Window to show Spherical photo
	/// </summary>
	public partial class PhotoViewWnd : MetroWindow
	{
        public event Action PrevPhotoRequested = null;
        public event Action NextPhotoRequested = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public PhotoViewWnd()
		{
			InitializeComponent();
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

		/// <summary>
		/// Set Image to show
		/// </summary>
		/// <param name="img"></param>
		public void SetImage (BitmapImage img)
		{
			// If the image is the URL, it takes while to get image.
			if (img.IsFrozen == false && img.PixelWidth == 1)
			{
				_progress.Visibility = Visibility.Visible;
				img.DownloadFailed += Img_DownloadFailed;
				img.DownloadCompleted += Img_DownloadCompleted;
			}
			else
			{
				_progress.Visibility = Visibility.Collapsed;
			}

			DoSetImage(img);
		}

		/// <summary>
		/// Set image to the control
		/// </summary>
		/// <param name="img"></param>
		private void DoSetImage(BitmapImage img)
		{
			if (img.Width / img.Height == 2.0)
			{
				viewSphere.Source = img;
				viewSphere.Visibility = Visibility.Visible;
				viewNormal.Visibility = Visibility.Collapsed;
			}
			else
			{
				viewNormal.Source = img;
				viewSphere.Visibility = Visibility.Collapsed;
				viewNormal.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Load image failed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Img_DownloadFailed(object sender, ExceptionEventArgs e)
		{
			_progress.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Load  image succeeded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Img_DownloadCompleted(object sender, EventArgs e)
		{
			_progress.Visibility = Visibility.Collapsed;
			DoSetImage(sender as BitmapImage);
		}

        /// <summary>
        /// Left button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            PrevPhotoRequested?.Invoke();
        }

        /// <summary>
        /// Right button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            NextPhotoRequested?.Invoke();
        }
    }
}
