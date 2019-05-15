using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class OptionValues
	{
		private const int I_DEFAULT = 999;
		private const double D_DEFAULT = 999.0;
		/// <summary>
		/// Aperture value. 
		/// </summary>
		/// <example>Fixed : 2.0</example>
		[DataMember(Name = "aperture", IsRequired = false, EmitDefaultValue = false)]
		public float Aperture { get; set; }

		/// <summary>
		/// Shooting interval (sec.) for interval shooting.
		/// </summary>
		/// <value> 5376 x 2688 : Min 8, Max 3600</value>
		/// <value> 2048 x 1024 : Min 5, Max 3600</value>
		[DataMember(Name = "captureInterval", IsRequired = false, EmitDefaultValue = false)]
		public int CaptureInterval { get; set; }

		/// <summary>
		/// API client version
		/// </summary>
		[DataMember(Name = "clientVersion", IsRequired = false, EmitDefaultValue = false)]
		public int ClientVersion { get; set; }

		/// <summary>
		/// The estimated remaining number of shots for the current shooting settings <br />
		/// Read only
		/// </summary>
		[DataMember(Name = "remainingPictures", IsRequired = false, EmitDefaultValue = false)]
		public int RemainingPictures { get; set; }

		/// <summary>
		/// The estimated remaining shooting time (sec.) for the current video shooting settings.  <br />
		/// Read only
		/// </summary>
		[DataMember(Name = "remainingVideoSeconds", IsRequired = false, EmitDefaultValue = false)]
		public long RemainingVideoSeconds { get; set; }

		/// <summary>
		/// Remaining usable storage space (byte).  <br />
		/// Read only
		/// </summary>
		[DataMember(Name = "remainingSpace", IsRequired = false, EmitDefaultValue = false)]
		public long RemainingSpace { get; set; }

		/// <summary>
		/// Total storage space (byte).  <br />
		/// Read only
		/// </summary>
		[DataMember(Name = "totalSpace", IsRequired = false, EmitDefaultValue = false)]
		public long TotalSpace { get; set; }

		/// <summary>
		/// White balance 
		/// </summary>
		[DataMember(Name = "whiteBalance", IsRequired = false, EmitDefaultValue = false)]
		public string WhiteBalance { get; set; }

		public enum EXPOSURE : int { MANUAL = 1, NORMAL = 2, SHUTTER = 4, ISO = 9 };

		/// <summary>
		/// Exposure program. The exposure settings that take priority can be selected. 
		/// </summary>
		[DataMember(Name = "exposureProgram", IsRequired = false, EmitDefaultValue = false)]
		public int ExposureProgram { get; set; }

		/// <summary>
		/// Shutter speed (sec.). 
		/// </summary>
		[DataMember(Name = "shutterSpeed", IsRequired = false, EmitDefaultValue = false)]
		public double ShutterSpeed { get; set; }

		/// <summary>
		/// ISO sensitibity. 
		/// </summary>
		[DataMember(Name = "iso", IsRequired = false, EmitDefaultValue = false)]
		public int ISO { get; set; }

		public const int SLEEP_DELAY_AUTO = 65535;
		public const int OFF_DELAY_AUTO = 65535;
		public const int WLAN_AUTO = 0;

		/// <summary>
		/// Standby time (sec.) until sleep. 
		/// </summary>
		[DataMember(Name = "sleepDelay", IsRequired = false, EmitDefaultValue = false)]
		public int SleepDelay { get; set; }

		/// <summary>
		/// Standby time (sec.) until the power automatically turns OFF. 
		/// </summary>
		[DataMember(Name = "offDelay", IsRequired = false, EmitDefaultValue = false)]
		public int OffDelay { get; set; }

		/// <summary>
		/// Wi-Fi channel. 
		/// </summary>
		[DataMember(Name = "_wlanChannel", IsRequired = false, EmitDefaultValue = false)]
		public int WiFiChannel { get; set; } = I_DEFAULT;


		/// <summary>
		/// Shutter volume. 
		/// </summary>
		[DataMember(Name = "_shutterVolume", IsRequired = false, EmitDefaultValue = false)]
		public int ShutterVolume { get; set; }

		/// <summary>
		/// Exposure compensation (EV). 
		/// </summary>
		[DataMember(Name = "exposureCompensation", IsRequired = false, EmitDefaultValue = false)]
		public double ExposureCompensation { get; set; } = D_DEFAULT;

		/// <summary>
		/// Shot image format. 
		/// </summary>
		[DataMember(Name = "fileFormat", IsRequired = false, EmitDefaultValue = false)]
		public FileFormat FileFormat { get; set; }

		/// <summary>
		/// Image processing filter. <br />
		/// Configured the filter will be applied while in still image shooting mode.However, it is disabled during continuous shooting. <br />
		/// When _filter is enabled, it takes priority over the exposure program (exposureProgram). <br /> 
		/// Also, when _filter is enabled, the exposure program is set to the Normal program.
		/// </summary>
		[DataMember(Name = "_filter", IsRequired = false, EmitDefaultValue = false)]
		public String ImageFilter { get; set; }

		/// <summary>
		/// Operating time (sec.) of the self-timer. <br />
		/// If exposureDelay is enabled, self-timer is used by shooting in still image capture mode. <br />
		/// If exposureDelay is disabled, use _latestEnabledExposureDelayTime to get the operating time of the self-timer stored in the camera. 
		/// </summary>
		[DataMember(Name = "exposureDelay", IsRequired = false, EmitDefaultValue = false)]
		public int ExposureDelay { get; set; } = I_DEFAULT;

		int _wlanChannel = 0;
		double _exposureCompensation = 0;
		int _exposureDelay = 0;
		[OnSerializing]
		void OnSerializing(StreamingContext context)
		{
			_wlanChannel = this.WiFiChannel;
			this.WiFiChannel = this.WiFiChannel == I_DEFAULT ? 0 : this.WiFiChannel == 0 ? I_DEFAULT : this.WiFiChannel;
			_exposureCompensation = this.ExposureCompensation;
			this.ExposureCompensation = this.ExposureCompensation == D_DEFAULT ? 0 : this.ExposureCompensation == 0 ? D_DEFAULT : this.ExposureCompensation;
			_exposureDelay = this.ExposureDelay;
			this.ExposureDelay = this.ExposureDelay == I_DEFAULT ? 0 : this.ExposureDelay == 0 ? I_DEFAULT : this.ExposureDelay;
		}
		[OnSerialized]
		void OnSerialized(StreamingContext context)
		{
			this.WiFiChannel = _wlanChannel;
			this.ExposureCompensation = _exposureCompensation;
			this.ExposureDelay = _exposureDelay;
		}
	}

	/// <summary>
	/// File format
	/// </summary>
	[DataContract]
	public class FileFormat
	{
		/// <summary>
		/// File format (Jpeg, Mp4) 
		/// </summary>
		[DataMember(Name = "type")]
		public string FileType { get; set; }

		/// <summary>
		/// File dimension (Width). 
		/// </summary>
		[DataMember(Name = "width")]
		public int Width { get; set; }

		/// <summary>
		/// File dimension (Height). 
		/// </summary>
		[DataMember(Name = "height")]
		public int Height { get; set; }

		#region Compare
		public override bool Equals(object obj)
		{
			var ff = obj as FileFormat;
			return ff != null
				&& ff.FileType.Equals(this.FileType) 
				&& ff.Width == this.Width 
				&& ff.Height == this.Height;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
	}
}
