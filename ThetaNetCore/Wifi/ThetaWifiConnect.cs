using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ThetaNetCore.Wifi.Resources;

namespace ThetaNetCore.Wifi
{
	public class ThetaWifiConnect
	{
		/// <summary>
		/// Check if connection to THETA is established
		/// </summary>
		/// <returns>Error message when something went wrong. Otherwise empty. </returns>
		async internal static Task CheckConnection(ThetaConnect _theta)
		{
			try
			{
				var info = await _theta.InfoAsync();
				if (info.ApiLevel == null || !info.ApiLevel.Contains(2))
				{
					throw new ApplicationException(AppStrings.Err_FirmwareVersion);

				}

				var state = await _theta.StateAsync();
				if (state.State.ApiVersion == 1)
				{
					// Change API to V2.1 
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
