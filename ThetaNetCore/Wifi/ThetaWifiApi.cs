using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ThetaNetCore.Resources;
using ThetaNetCore.Util;

namespace ThetaNetCore.Wifi
{
	public partial class ThetaWifiApi
	{
		private const string THETA_URL = "http://192.168.1.1:80/";
		private enum SEND_TYPE { GET, POST };


		#region Utility, Common Functions
		/// <summary>
		/// Get the camera statuses
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<StateResponse> StateAsync()
		{
			var response = await SendRequestAsync<Object>(SEND_TYPE.POST, null, "osc/state");
			CheckResponse(response);
			return JsonUtil.ToObject<StateResponse>(response.GetResponseStream());
		}

		/// <summary>
		/// Get the camera statuses
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<String> CheckForUpdateAsync(String fingerPrint)
		{
			var command = new CheckForUpdateRequest() { StateFingerprint = fingerPrint };
			var response = await SendRequestAsync<CheckForUpdateRequest>(SEND_TYPE.POST, command, "osc/checkForUpdates");
			CheckResponse(response);
			var resObj = JsonUtil.ToObject<CheckForUpdateResponse>(response.GetResponseStream());
			return resObj.StateFingerprint;
		}

		#endregion

		/// <summary>
		/// Get basic information for camera and supported functions
		/// </summary>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<InfoResponse> InfoAsync()
		{
			var response = await SendRequestAsync<Object>(SEND_TYPE.GET, null, "osc/info");
			CheckResponse(response);
			return JsonUtil.ToObject<InfoResponse>(response.GetResponseStream());
		}

		/// <summary>
		/// Starts session
		/// </summary>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<StartSessionResponse> StartSessionAsync()
		{
			var command = new ThetaRequest<Object>() { Name = "camera.startSession" };
			var response = await ExecuteCommandAsync<Object>(command);
			CheckResponse(response);
			return JsonUtil.ToObject<ThetaResponse<StartSessionResponse>>(response.GetResponseStream()).Results;
		}

		/// <summary>
		/// Close session
		/// </summary>
		async public Task CloseSessionAsync(String sessionId)
		{
			var command = new ThetaRequest<SessionParam>()
			{ Name = "camera.closeSession", Parameters = new SessionParam() { SessionId = sessionId } };
			using (var response = await ExecuteCommandAsync<SessionParam>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Get Option values
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<OptionValues> GetOptionsAsync(GetOptionsParam request)
		{
			var optionParams = new GetOptionParams() { Options = request.OptionNames };
			var command = new ThetaRequest<GetOptionParams>() { Name = "camera.getOptions", Parameters = optionParams };
			try
			{
				var response = await ExecuteCommandAsync<GetOptionParams>(command);
				CheckResponse(response);
				var res = JsonUtil.ToObject<ThetaResponse<SetOptionParams>>(response.GetResponseStream());
				return res.Results.Options;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);
				return null;
			}
		}

		/// <summary>
		/// Set option parameters
		/// </summary>
		/// <param name="optionValues"></param>
		/// <param name="sessionId">if current api is 2.0, session ID is required to change to 2.1</param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task SetOptionsAsync(OptionValues optionValues, String sessionId = null)
		{
			var optionParams = new SetOptionParams() { Options = optionValues };
			if (sessionId != null)
				optionParams.SessionId = sessionId;
			var command = new ThetaRequest<SetOptionParams>()
			{ Name = "camera.setOptions", Parameters = optionParams };
			using (var response = await ExecuteCommandAsync<SetOptionParams>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Turns the wireless LAN off.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task FinishWlan()
		{
			var command = new ThetaRequest<Object>() { Name = "camera._finishWlan" };
			using (var response = await ExecuteCommandAsync<Object>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Reset all device settings and capture settings. <br />
		/// After reset, the camera will be restarted.
		/// </summary>
		/// <returns></returns>
		async public Task Reset()
		{
			var command = new ThetaRequest<Object>() { Name = "camera.reset" };
			using (var response = await ExecuteCommandAsync<Object>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Stop running self-timer. <br />
		/// If self-timer(exposureDelay) is enabled, self-timer shooting(camera.takePicture) is started.
		/// </summary>
		/// <returns></returns>
		async public Task StopSelfTimer()
		{
			var command = new ThetaRequest<Object>() { Name = "camera._stopSelfTimer" };
			using (var response = await ExecuteCommandAsync<Object>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Starts getting preview images
		/// </summary>
		async public Task<Stream> GetLivePreviewAsync()
		{
			var command = new ThetaRequest<Object>() { Name = "camera.getLivePreview" };
			var response = await ExecuteCommandAsync<Object>(command, 3000);
			CheckResponse(response);
			return response.GetResponseStream();
		}

		/// <summary>
		/// Starts still image shooting
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<TakePictureResponse> TakePictureAsync()
		{
			var state = await StateAsync();
			var command = new ThetaRequest<Object>() { Name = "camera.takePicture" };
			var response = await ExecuteCommandAsync<Object>(command);
			CheckResponse(response);
			return JsonUtil.ToObject<TakePictureResponse>(response.GetResponseStream());
		}

		/// <summary>
		/// List files in the camera device
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task<ListFilesResponse> ListFilesAsync(ListFilesParam param)
		{
			var command = new ThetaRequest<ListFilesParam>()
			{ Name = "camera.listFiles", Parameters = param };
			var response = await ExecuteCommandAsync<ListFilesParam>(command);
			CheckResponse(response);
			return JsonUtil.ToObject<ThetaResponse<ListFilesResponse>>(response.GetResponseStream()).Results;
		}

		/// <summary>
		/// Delete an image on the camera
		/// </summary>
		/// <param name="fileUrl"></param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaWifiApiException"></exception> 
		async public Task DeleteAsync(String[] fileUrls)
		{
			var command = new ThetaRequest<DeleteParam>()
			{ Name = "camera.delete", Parameters = new DeleteParam() { FileUrls = fileUrls } };
			using (var response = await ExecuteCommandAsync<DeleteParam>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Get a binary image data from the camera
		/// </summary>
		/// <param name="fileUrl"></param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
		async public Task<Stream> GetImageAsync(String fileUrl)
		{
			var response = await DownloadFileAsync(fileUrl);
			CheckResponse(response);
			return response.GetResponseStream();
		}

		/// <summary>
		/// Download file
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		async private static Task<HttpWebResponse> DownloadFileAsync(String uri)
		{
			HttpWebRequest request = System.Net.WebRequest.Create(uri) as HttpWebRequest;
			request.Method = "GET";

			return await request.GetResponseAsync() as HttpWebResponse;
		}

		/// <summary>
		/// Gets a list of installed plugins.
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		async public Task<ListPluginsResponse> ListPluginsAsync()
		{
			var command = new ThetaRequest<Object>() { Name = "camera._listPlugins" };
			var response = await ExecuteCommandAsync<Object>(command);
			CheckResponse(response);
			return JsonUtil.ToObject<ListPluginsResponse>(response.GetResponseStream());
		}

		/// <summary>
		/// Sets the installed pugin for boot. <br />
		/// For RICOH THETA Z1 or later, this command is ignored.Use camera._pluginControl
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		async public Task SetPluginsAsync(String packageName)
		{
			var command = new ThetaRequest<SetPluginParam>()
			{
				Name = "camera._setPlugin",
				Parameters = new SetPluginParam() { PackageName = packageName }
			};
			using (var response = await ExecuteCommandAsync<SetPluginParam>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Starts or stops plugin.
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		async public Task PluginControlAsync(PLUGIN_ACTION action, String pluginName = null)
		{
			var command = new ThetaRequest<PluginControlParam>()
			{
				Name = "camera._pluginControl",
				Parameters = new PluginControlParam() { PluginAction = action, Plugin = pluginName }
			};
			using (var response = await ExecuteCommandAsync<PluginControlParam>(command))
				CheckResponse(response);
		}
	}

}
