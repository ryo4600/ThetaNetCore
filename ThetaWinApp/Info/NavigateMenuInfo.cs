using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ThetaWinApp.Info
{
	public class NavigateMenuItem
	{
		public String Title { get; set; }
		public ImageSource Icon { get; set; }
		public PackIconKind PackIconKind { get; set; }
		public String NavigateTo { get; set; }
	}

	public class NavigateMenuItems : ObservableCollection<NavigateMenuItem> { }

}
