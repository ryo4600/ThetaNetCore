using System;

namespace ThetaNetCore.Wifi
{
	/// <summary>
	/// Theta related exceptions are wrapped with this class
	/// </summary>
	public class ThetaException : Exception
	{
		public ErrorResponse Error { get; private set;}

		public ThetaException(ErrorResponse response) : base(response.Error.Message)
		{
			Error = response;
		}
	}
}
