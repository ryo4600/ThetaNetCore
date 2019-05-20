using System;

namespace ThetaNetCore.Wifi
{
	/// <summary>
	/// Exception created by the Theta connection wrapper
	/// </summary>
	public class ThetaWifiConnectException : Exception
	{
		public ThetaWifiConnectException(String message) : base(message)
		{
		}

		public ThetaWifiConnectException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
