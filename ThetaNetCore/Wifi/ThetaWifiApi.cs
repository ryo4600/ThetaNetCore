using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ThetaNetCore.Util;

namespace ThetaNetCore.Wifi
{
	public class ThetaConnect
	{
		private const string THETA_URL = "http://192.168.1.1:80/";
		private enum SEND_TYPE { GET, POST };

		#region Events
		/// <summary>
		/// Notifies when the taking picture operation is completed
		/// </summary>
		public event Action<String> TakePictureCompleted = null;

		/// <summary>
		/// Called when the taking picture operation failed
		/// </summary>
		public event Action<Exception> TakePictureFailed = null;

		/// <summary>
		/// Called when the taking picture is initiated by pressing hard button.
		/// </summary>
		public event Action HardButtonPressed = null;

		#endregion

		/// <summary>
		/// Get basic information for camera and supported functions
		/// </summary>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
		async public Task<InfoResponse> InfoAsync()
		{
			var response = await SendRequestAsync<Object>(SEND_TYPE.GET, null, "osc/info");
			CheckResponse(response);
			return JsonUtil.ToObject<InfoResponse>(response.GetResponseStream());
		}

		/// <summary>
		/// Get the camera statuses
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
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
		/// <exception cref="ThetaException"></exception> 
		async public Task<String> CheckForUpdateAsync(String fingerPrint)
		{
			var command = new CheckForUpdateRequest() { StateFingerprint = fingerPrint };
			var response = await SendRequestAsync<CheckForUpdateRequest>(SEND_TYPE.POST, command, "osc/checkForUpdates");
			CheckResponse(response);
			var resObj = JsonUtil.ToObject<CheckForUpdateResponse>(response.GetResponseStream());
			return resObj.StateFingerprint;
		}

		/// <summary>
		/// Throws excepton if response code indicates an error
		/// </summary>
		/// <param name="response"></param>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
		private static void CheckResponse(HttpWebResponse response)
		{
			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return;
				case HttpStatusCode.BadRequest:				// 400
				case HttpStatusCode.Forbidden:				// 403
				case HttpStatusCode.ServiceUnavailable:		// 503:
					{
						var errRes = JsonUtil.ToObject<ErrorResponse>(response.GetResponseStream());
						throw new ThetaException(errRes);
					}
				default:
					throw new Exception(String.Format(Wifi.Resources.AppStrings.Err_SendRequestFailed, response.StatusCode));
			}
		}

		/// <summary>
		/// Starts session
		/// </summary>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
		async public Task<StartSessionResponse> StartSessionAsync()
		{
			var command = new ThetaRequest<Object>() { Name = "camera.startSession" };
			var response = await ExecuteCommandAsync<Object>(command);
			CheckResponse(response);
			return JsonUtil.ToObject<ThetaResponse<StartSessionResponse>>(response.GetResponseStream()).Results;
		}

		/// <summary>
		/// Get Option values
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
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
		/// <exception cref="ThetaException"></exception> 
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
		/// <exception cref="ThetaException"></exception> 
		async public Task FinishWlan()
		{
			var command = new ThetaRequest<Object>() { Name = "camera._finishWlan" };
			using (var response = await ExecuteCommandAsync<Object>(command))
				CheckResponse(response);
		}

		/// <summary>
		/// Starts still image shooting
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
		async public Task<TakePictureResponse> TakePictureAsync()
		{
			var state = await StateAsync();
			var command = new ThetaRequest<Object>() { Name = "camera.takePicture" };
			var response = await ExecuteCommandAsync<Object>(command);
			CheckResponse(response);
			StartCheckPictureCompletion(state.FingerPrint, state.State.LatestFileUrl);
			return JsonUtil.ToObject<TakePictureResponse>(response.GetResponseStream());
		}

		/// <summary>
		/// Watches for the taking picture completion
		/// </summary>
		/// <param name="fingerPrint"></param>
		private void StartCheckPictureCompletion(string fingerPrint, string lastFileUrl)
		{
			Task.Factory.StartNew(new Action(async () =>
			{
				try
				{
					while (true)
					{
						var newId = await CheckForUpdateAsync(fingerPrint);
						if (newId != fingerPrint)
						{
							var newState = await StateAsync();
							if (newState.State.LatestFileUrl != lastFileUrl)
							{
								if (TakePictureCompleted != null)
									TakePictureCompleted(newState.State.LatestFileUrl);
								break;
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (TakePictureFailed != null)
						TakePictureFailed(ex);
				}
			}));
		}

		/// <summary>
		/// List files in the camera device
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		/// <exception cref="SerializationException"></exception>
		/// <exception cref="ThetaException"></exception> 
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
		/// <exception cref="ThetaException"></exception> 
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

		#region Live Preview
		/// <summary>
		/// Notifies when an image is ready
		/// </summary>
		public event Action<byte[]> ImageReady;

		/// <summary>
		/// Flag to stop the live preview
		/// </summary>
		private bool _stopPreview = false;

		/// <summary>
		/// Previewing ?
		/// </summary>
		private bool _isPreviewing = false;

		/// <summary>
		/// Starts getting preview images
		/// </summary>
		async public Task StartLivePreviewAsync()
		{
			var command = new ThetaRequest<Object>() { Name = "camera.getLivePreview" };
			var response = await ExecuteCommandAsync<Object>(command, 30000);
			CheckResponse(response);

			var state = await StateAsync();

			await Task.Factory.StartNew(new Action(() =>
		   {
			   var stream = response.GetResponseStream();

			   BinaryReader reader = new BinaryReader(stream, new System.Text.ASCIIEncoding());
			   var imageBytes = new System.Collections.Generic.List<byte>();
			   bool loadStarted = false; // 画像の頭のバイナリとったかフラグ
			   byte oldByte = 0; // 1つ前のByteデータを格納する
			   _isPreviewing = true;

			   while (true)
			   {
				   if (_stopPreview)
				   {
					   _stopPreview = false;
					   break;
				   }

				   try
				   {
					   byte byteData = reader.ReadByte();

					   if (!loadStarted)
					   {
						   if (oldByte == 0xFF)
						   {
							   // First binary of the image
							   imageBytes.Add(0xFF);
						   }
						   if (byteData == 0xD8)
						   {
							   // Second binary
							   imageBytes.Add(0xD8);
							   loadStarted = true;
						   }
					   }
					   else
					   {
						   imageBytes.Add(byteData);

						   // checkes end bytes sequence
						   if (oldByte == 0xFF && byteData == 0xD9)
						   {
							   if (ImageReady != null)
								   ImageReady(imageBytes.ToArray());

							   imageBytes.Clear();
							   // read the next image
							   loadStarted = false;
						   }
					   }
					   oldByte = byteData;
				   }
				   catch (System.IO.EndOfStreamException)
				   {
					   Task.Factory.StartNew(new Action(() =>
					   {
						   // Picture is taken with the hard button
						   if (HardButtonPressed != null)
							   HardButtonPressed();
						   StartCheckPictureCompletion(state.FingerPrint, state.State.LatestFileUrl);
					   }));
					   break;
				   }
				   catch
				   {
					  // throw new ThetaException(
					   break;
				   }
			   }
			   try
			   {
				   stream.Dispose();
			   }
			   catch
			   {
			   }
			   _isPreviewing = false;
		   }));

		}

		/// <summary>
		/// Ends getting preview images
		/// </summary>
		public async Task StopLivePreviewAsync()
		{
			if (!_isPreviewing)
				return;

			_stopPreview = true;

			while (!_isPreviewing)
			{
				await Task.Delay(100);
			}
		}
		#endregion

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

		async private static Task<HttpWebResponse> DownloadFileAsync(String uri)
		{
			HttpWebRequest request = System.Net.WebRequest.Create(uri) as HttpWebRequest;
			request.Method = "GET";

			return await request.GetResponseAsync() as HttpWebResponse;
		}
	}
}
