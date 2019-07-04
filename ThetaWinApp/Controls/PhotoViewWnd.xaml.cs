using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Media;

namespace ThetaWinApp.Controls
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
		/// Set Image to show
		/// </summary>
		/// <param name="img"></param>
		public void SetImage (ImageSource img)
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
