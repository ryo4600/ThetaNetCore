using System;
using System.Windows;
using System.Windows.Controls;
using ThetaWinApp.Info;
using System.Linq;

namespace ThetaWinApp.Controls.Home
{
	/// <summary>
	/// Home window
	/// </summary>
	public partial class HomeCtrl : UserControl
	{
		/// <summary>
		/// Register handler for tile tap events.
		/// </summary>
		public event Action<NavigateMenuItem> MenuSelected = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public HomeCtrl()
		{
			InitializeComponent();

			// Remove first item "home", which is this page
			var items =  this.FindResource("DrawerItems") as NavigateMenuItems;
			lstTiles.DataContext = items.Skip(1);
		}

		/// <summary>
		/// Tile is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TileButton_Click(object sender, RoutedEventArgs e)
		{
			var info = ((FrameworkElement)e.Source).DataContext as NavigateMenuItem;

			if (info != null && MenuSelected != null)
				MenuSelected(info);
		}
	}
}
