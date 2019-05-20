using System;
using System.Collections.Generic;
using System.Text;

namespace ThetaNetCore.Wifi
{
	/// <summary>
	/// Theta related exceptions are wrapped with this class
	/// </summary>
	public class ThetaWifiApiException : Exception
	{
		public ErrorResponse Error { get; private set; }

		public ThetaWifiApiException(ErrorResponse response) : base(response.Error.Message)
		{
			Error = response;
		}
	}
}
