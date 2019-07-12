using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ThetaNetCore.Resources;

namespace ThetaNetCore.Wifi
{
	/// <summary>
	/// Theta connection wrapper
	/// </summary>
	public class ThetaWifiConnect
	{
		#region Events
		/// <summary>
		/// Notifies when the taking picture operation is completed
		/// </summary>
		public event Action<String> OnTakePictureCompleted = null;

		/// <summary>
		/// Called when the taking picture operation failed
		/// </summary>
		public event Action<Exception> OnTakePictureFailed = null;

		/// <summary>
		/// Called when the taking picture is initiated by pressing hard button.
		/// </summary>
		public event Action OnHardButtonPressed = null;

		/// <summary>
		/// Preview is terminated unexpectedly.
		/// </summary>
		public event Action<ThetaWifiConnectException> OnPreviewTerminated = null;
		#endregion

		private readonly ThetaWifiApi _theta = new ThetaWifiApi();
		/// <summary>
		/// If fine control is needed, use this instance.
		/// </summary>
		public ThetaWifiApi ThetaApi { get => _theta; }
		public bool IsPreviewing { get => _isPreviewing; }

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
					throw new ApplicationException(WifiStrings.Err_FirmwareVersion);

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
				throw new ThetaWifiConnectException(WifiStrings.Err_ConnectionFailed, wex);
			}
			catch (SerializationException sex)
			{
				throw new ThetaWifiConnectException(WifiStrings.Err_ConnectionFailed, sex);
			}
		}

		#region Live preview related members
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
		#endregion

		#region Live Preview
		/// <summary>
		/// Starts getting preview images
		/// </summary>
		public void StartLivePreview()
		{
			if (_isPreviewing)
				return;

			_isPreviewing = true;
			Task.Factory.StartNew(new Action(async() =>
			{
				var state = await _theta.StateAsync();
				var stream = await _theta.GetLivePreviewAsync();

				var reader = new BinaryReader(stream, new System.Text.ASCIIEncoding());
				var imageBytes = new System.Collections.Generic.List<byte>();
				bool loadStarted = false; // 画像の頭のバイナリとったかフラグ
				byte oldByte = 0; // 1つ前のByteデータを格納する
				using (stream)
				{
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
							// Stream ends when picture is taken from the hard button
							if (OnHardButtonPressed != null)
								OnHardButtonPressed();
							BeginWatchingForCompletion(state.FingerPrint, state.State.LatestFileUrl);
							break;
						}
						catch (Exception ex)
						{
							OnPreviewTerminated?.Invoke(new ThetaWifiConnectException(WifiStrings.Err_GetImage, ex));
							break;
						}
					}
				}
				_isPreviewing = false;
			}));

		}

		/// <summary>
		/// Watches for the taking picture completion
		/// </summary>
		/// <param name="fingerPrint"></param>
		private void BeginWatchingForCompletion(string fingerPrint, string lastFileUrl)
		{
			Task.Factory.StartNew(new Action(async () =>
			{
				try
				{
					while (true)
					{
						var newId = await _theta.CheckForUpdateAsync(fingerPrint);
						if (newId != fingerPrint)
						{
							var newState = await _theta.StateAsync();
							if (newState.State.LatestFileUrl != lastFileUrl)
							{
								if (OnTakePictureCompleted != null)
									OnTakePictureCompleted(newState.State.LatestFileUrl);
								break;
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (OnTakePictureFailed != null)
						OnTakePictureFailed(ex);
				}
			}));
		}


		/// <summary>
		/// Ends getting preview images
		/// </summary>
		public void StopLivePreview()
		{
			if (!_isPreviewing)
				return;

			_stopPreview = true;
		}

		#endregion

		/// <summary>
		/// Start taking picture
		/// </summary>
		/// <returns></returns>
		public async Task<TakePictureResponse> TakePictureAsync()
		{
			_isPreviewing = false;

			var response = await _theta.TakePictureAsync();
			var state = await _theta.StateAsync();

			BeginWatchingForCompletion(state.FingerPrint, state.State.LatestFileUrl);

			return response;
		}
	}

}
