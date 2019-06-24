using System;
using System.Windows;
using System.Windows.Controls;
using ThetaWinApp.Info;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaWinApp.Controls
{
	public sealed partial class LocalImageCtrl : UserControl
	{
		#region events
		public event Action<ImageFileWrapper> ControlLoaded = null;
		public event Action<ImageFileWrapper> ViewImageRequested = null;
		public event Action<ImageFileWrapper> DeleteRequested = null;

		#endregion

		public bool ShowButtons
		{
			get { return (bool)GetValue(ShowButtonsProperty); }
			set
			{
				SetValue(ShowButtonsProperty, value);
				ButtonArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		// Using a DependencyProperty as the backing store for ShowButtons.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShowButtonsProperty =
			DependencyProperty.Register("ShowButtons", typeof(bool), typeof(DeviceImageCtrl), new PropertyMetadata(true));

		/// <summary>
		/// Constructor
		/// </summary>
		public LocalImageCtrl()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Download button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnView_Click(object sender, RoutedEventArgs e)
		{
			if (ViewImageRequested != null)
				ViewImageRequested(this.DataContext as ImageFileWrapper);
		}

		/// <summary>
		/// Delete button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (DeleteRequested != null)
				DeleteRequested(this.DataContext as ImageFileWrapper);
		}

		/// <summary>
		/// DataContext Changed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (ControlLoaded != null)
				ControlLoaded(this.DataContext as ImageFileWrapper);
		}


		private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (ViewImageRequested != null)
				ViewImageRequested(this.DataContext as ImageFileWrapper);

		}

	}
}
