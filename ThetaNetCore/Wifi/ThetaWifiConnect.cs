using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ThetaNetCore.Wifi.Resources;

namespace ThetaNetCore.Wifi
{
	public class ThetaWifiConnect
	{
		private readonly ThetaWifiApi _theta = new ThetaWifiApi();
		/// <summary>
		/// If fine control is needed, use this instance.
		/// </summary>
		public ThetaWifiApi ThetaApi { get => _theta; }

		/// <summary>
		/// Check if connection to THETA is established
		/// </summary>
		/// <returns>Error message when something went wrong. Otherwise empty. </returns>
		async public Task CheckConnection()
		{
			try
			{
				var info = await _theta.InfoAsync();
				// 1 = v2.0, 2 = v2.1
				// This library needs v2.1 support
				if (info.ApiLevel == null || !info.ApiLevel.Contains(2))
				{
					throw new ApplicationException(AppStrings.Err_FirmwareVersion);

				}

				// If current setting is 2.0, change to 2.1
				var state = await _theta.StateAsync();
				if (state.State.ApiVersion == 1)
				{
					var session = await _theta.StartSessionAsync();
					await _theta.SetOptionsAsync(new OptionValues() { ClientVersion = 2 }, session.SessionId);
				}
			}
			catch (System.Net.WebException wex)
			{
				throw new ApplicationException(AppStrings.Err_ConnectionFailed, wex);
			}
			catch (SerializationException sex)
			{
				throw new ApplicationException(AppStrings.Err_ConnectionFailed, sex);
			}
		}

	}
}
