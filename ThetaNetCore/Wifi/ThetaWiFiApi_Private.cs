using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThetaNetCore.Resources;
using ThetaNetCore.Util;

namespace ThetaNetCore.Wifi
{
	/// <summary>
	/// Basically, all private methods are written here.
	/// </summary>
	public partial class ThetaWifiApi
	{
		/// <summary>
		/// Common function to send a command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		async private static Task<HttpWebResponse> ExecuteCommandAsync<T>(ThetaRequest<T> command, int? timeout = null)
		{
			return await SendRequestAsync(SEND_TYPE.POST, command, "osc/commands/execute", timeout);
		}

		/// <summary>
		/// Common function to send a command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		async private static Task<HttpWebResponse> ExecuteStatusAsync<T>(ThetaRequest<T> command)
		{
			return await SendRequestAsync(SEND_TYPE.POST, command, "osc/commands/status");
		}

		/// <summary>
		/// Send a request to Theta <br />
		/// Actual network operations are done here.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sendType">POST/GET</param>
		/// <param name="command"></param>
		/// <param name="commandPath"></param>
		/// <returns></returns>
		async private static Task<HttpWebResponse> SendRequestAsync<T>(SEND_TYPE sendType, T command, String commandPath, int? timeout = null)
		{
			HttpWebRequest request = System.Net.WebRequest.Create(Path.Combine(THETA_URL, commandPath)) as HttpWebRequest;
			request.Method = sendType == SEND_TYPE.POST ? "POST" : "GET";
			request.Accept = "application/json";
			request.ContentType = "application/json";
			if (timeout != null)
				request.ContinueTimeout = timeout.Value;

			if (command != null)
			{
				var jsonString = JsonUtil.ToSring<T>(command).Replace(":999", ":0");
				var reqStream = await request.GetRequestStreamAsync();
				byte[] buffer = UTF8Encoding.UTF8.GetBytes(jsonString);
				reqStream.Write(buffer, 0, buffer.Length);
				reqStream.Flush();
			}

			return await request.GetResponseAsync() as HttpWebResponse;
		}

		/// <summary>
		/// Throws excepton if response code indicates an error
		/// </summary>
		/// <param name="response"></param>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		private static void CheckResponse(HttpWebResponse response)
		{
			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return;
				case HttpStatusCode.BadRequest:             // 400
				case HttpStatusCode.Forbidden:              // 403
				case HttpStatusCode.ServiceUnavailable:     // 503:
					{
						var errRes = JsonUtil.ToObject<ErrorResponse>(response.GetResponseStream());
						throw new ThetaWifiApiException(errRes);
					}
				default:
					throw new Exception(String.Format(WifiStrings.Err_SendRequestFailed, response.StatusCode));
			}
		}

	}
}
