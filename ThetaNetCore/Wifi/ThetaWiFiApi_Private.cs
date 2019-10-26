using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
		/// HttpClient has to be reused according to the document.
		/// </summary>
		static readonly HttpClient _httpClient = new HttpClient();

		/// <summary>
		/// Common function to send a command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		async private static Task<HttpResponseMessage> ExecuteCommandAsync<T>(ThetaRequest<T> command, int? timeout = null)
		{
			return await SendRequestAsync(SEND_TYPE.POST, command, "osc/commands/execute", timeout);
		}

		/// <summary>
		/// Common function to send a command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		async private static Task<HttpResponseMessage> ExecuteStatusAsync<T>(ThetaRequest<T> command)
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
		/// <exception cref="ThetaWifiConnectException" />
		async private static Task<HttpResponseMessage> SendRequestAsync<T>(SEND_TYPE sendType, T command, String commandPath, int? timeout = null)
		{
			var httpClient = _httpClient;
			//httpClient.Timeout = new TimeSpan(0, 0, 5);
			var headers = httpClient.DefaultRequestHeaders;
			headers.Clear();
			headers.Add("Accept", "application/json");

			StringContent content = null;
			if (command != null)
			{
				var jsonString = JsonUtil.ToSring<T>(command).Replace(":999", ":0");
				content = new StringContent(jsonString, Encoding.UTF8, "application/json");
			}

			try
			{
				var commandUri = Path.Combine(THETA_URL, commandPath);
				if (sendType == SEND_TYPE.POST)
					return await httpClient.PostAsync(commandUri, content);
				else
					return await httpClient.GetAsync(commandUri);
			}
			catch (HttpRequestException wex)
			{
				throw new ThetaWifiConnectException(WifiStrings.Err_ConnectionFailed, wex);
			}
			catch (TaskCanceledException wex)
			{
				throw new ThetaWifiConnectException(WifiStrings.Err_ConnectionFailed, wex);
			}

		}

		/// <summary>
		/// Throws excepton if response code indicates an error<br/>
		/// Version with HttpClient -> HttpResponseMessage
		/// </summary>
		/// <param name="response"></param>
		async private static Task CheckResponseAsync(HttpResponseMessage response)
		{
			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return;
				case HttpStatusCode.BadRequest:             // 400
				case HttpStatusCode.Forbidden:              // 403
				case HttpStatusCode.ServiceUnavailable:     // 503:
					{
						var errRes = JsonUtil.ToObject<ErrorResponse>(await response.Content.ReadAsStreamAsync());
						throw new ThetaWifiConnectException(errRes.Error.Code + ":" + errRes.Error.Message);
					}
				default:
					throw new ThetaWifiConnectException(String.Format(WifiStrings.Err_SendRequestFailed, response.StatusCode));
			}
		}

		/// <summary>
		/// Requesting a video stream to theta <br />
		/// It is using HttpWebRequest because HttpClient.PostAsync never returns.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sendType">POST/GET</param>
		/// <param name="command"></param>
		/// <param name="commandPath"></param>
		/// <returns></returns>
		async private static Task<HttpWebResponse> RequestVideoStreamAsync()
		{
			var command = new ThetaRequest<Object>() { Name = "camera.getLivePreview" };

			HttpWebRequest request = System.Net.WebRequest.Create(Path.Combine(THETA_URL, "osc/commands/execute")) as HttpWebRequest;
			request.Method = "POST";
			request.Accept = "application/json";
			request.ContentType = "application/json";
			request.ContinueTimeout = 3000;

			var jsonString = JsonUtil.ToSring<ThetaRequest<Object>>(command).Replace(":999", ":0");
			var reqStream = await request.GetRequestStreamAsync();
			byte[] buffer = UTF8Encoding.UTF8.GetBytes(jsonString);
			reqStream.Write(buffer, 0, buffer.Length);
			reqStream.Flush();

			return await request.GetResponseAsync() as HttpWebResponse;
		}

		/// <summary>
		/// Throws excepton if response code indicates an error <br/>
		/// Version with HttpWebRequest -> HttpWebResponse
		/// </summary>
		/// <param name="response"></param>
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
						throw new ThetaWifiConnectException(errRes.Error.Code + ":" + errRes.Error.Message);
					}
				default:
					throw new ThetaWifiConnectException(String.Format(WifiStrings.Err_SendRequestFailed, response.StatusCode));
			}
		}

	}
}
