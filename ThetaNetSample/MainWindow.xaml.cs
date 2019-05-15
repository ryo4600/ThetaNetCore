using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThetaNetCore.Wifi;

namespace ThetaNetSample
{
	/// <summary>
	/// Main window
	/// </summary>
	public partial class MainWindow : Window
	{
		ThetaWifiConnect _theta = new ThetaWifiConnect();

		public MainWindow()
		{
			InitializeComponent();
		}

		private async void BtnInit_Click(object sender, RoutedEventArgs e)
		{
			string result;
			try {
				await _theta.CheckConnection();
				result = "Connection OK";
			}
			catch(ThetaException thex)
			{
				// Theta API erros... 
				result = thex.Message;
	
			}
			catch (ApplicationException appex)
			{
				// Connecting error...
				result = appex.Message;
			}

			txtOutput.Text += result + "\n";
		}
	}
}
