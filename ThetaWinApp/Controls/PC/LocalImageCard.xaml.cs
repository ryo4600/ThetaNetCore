using System;
using System.Windows;
using System.Windows.Controls;
using ThetaWinApp.Controls.Camera;
using ThetaWinApp.Info;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ThetaWinApp.Controls.PC
{
	public sealed partial class LocalImageCard : UserControl
	{
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
			DependencyProperty.Register("ShowButtons", typeof(bool), typeof(LocalImageCard), new PropertyMetadata(true));

		/// <summary>
		/// Constructor
		/// </summary>
		public LocalImageCard()
		{
			this.InitializeComponent();
		}
	}
}
