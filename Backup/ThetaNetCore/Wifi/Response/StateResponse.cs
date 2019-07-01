using System;
using System.Runtime.Serialization;
using ThetaNetCore.Resources;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class StateResponse
	{
		[DataMember(Name = "fingerprint")]
		public string FingerPrint { get; set; }

		[DataMember(Name = "state")]
		public StateResponseDetail State { get; set; }

	}

	[DataContract]
	public class StateResponseDetail
	{
#if v2
		[DataMember(Name = "sessionId")]
		public String SessionId { get; set; }
#endif

		[DataMember(Name = "batteryLevel")]
		public float BatteryLevel { get; set; }

		[DataMember(Name = "storageChanged")]
		public bool StorageChanged { get; set; }

		[DataMember(Name = "_captureStatus")]
		public String CaptureStatus { get; set; }

		[DataMember(Name = "_recordedTime")]
		public int RecordedTime { get; set; }

		[DataMember(Name = "_recordableTime")]
		public int RecordableTime { get; set; }

		[DataMember(Name = "_latestFileUrl")]
		public String LatestFileUrl { get; set; }

		[DataMember(Name = "_batteryState")]
		public String BatteryState { get; set; }

		/// <summary> API version is supported after V2.1 (=2) </summary>
		[DataMember(Name = "_apiVersion", IsRequired = false)]
		public int ApiVersion { get; set; }

		[DataMember(Name = "_cameraError")]
		public uint CameraErrorValue { get; set; }

		[IgnoreDataMember]
		public CAMERA_ERROR CameraError
		{
			get
			{
				foreach (CAMERA_ERROR err in Enum.GetValues(typeof(CAMERA_ERROR)))
				{
					if (CameraErrorValue == (uint)err)
						return err;
				}
				return CAMERA_ERROR.UNKNOWN;
			}
		}

		[IgnoreDataMember]
		public String CaptureStatusFriendly
		{
			get
			{
				switch (CaptureStatus)
				{
					case "shooting":
						return ApiStrings.CaptureStatus_Shooting;
					case "idle":
						return ApiStrings.CaptureStatus_Idle;
					case "self-timer countdown":
						return ApiStrings.CaptureStatus_SelfTimer;
					case "bracket shooting":
						return ApiStrings.CaptureStatus_BracketShooting;
					default:
						return ApiStrings.Invalid_Value;
				}

			}
		}

		[IgnoreDataMember]
		public String BatteryStateFriendly
		{
			get
			{
				switch (BatteryState)
				{
					case "charging":
						return ApiStrings.BatteryState_Charging;
					case "charged":
						return ApiStrings.BatteryState_Charged;
					case "disconnect":
						return ApiStrings.BatteryState_Disconnect;
					default:
						return ApiStrings.Invalid_Value;
				}
			}
		}

		[IgnoreDataMember]
		public String CameraErrorFriendly
		{
			get
			{
				switch(CameraError)
				{
					case CAMERA_ERROR.NONE:
						return ApiStrings.CameraError_None;
					case CAMERA_ERROR.NO_MEMORY:
						return ApiStrings.CameraError_NoMemory;
					case CAMERA_ERROR.WRITING_DATA:
						return ApiStrings.CameraError_WritingData;
					case CAMERA_ERROR.FILE_NUMBER_OVER:
						return ApiStrings.CameraError_FileNumberError;
					case CAMERA_ERROR.NO_DATE_SETTING:
						return ApiStrings.CameraError_NoDateSetting;
					case CAMERA_ERROR.COMPASS_CALIBRATION:
						return ApiStrings.CameraError_CompassCalibration;
					case CAMERA_ERROR.CARD_DETECT_FAIL:
						return ApiStrings.CameraError_CardDetectFail;
					case CAMERA_ERROR.CAPTURE_HW_FAILED:
						return ApiStrings.CameraError_CaptureHwFailed;
					case CAMERA_ERROR.CANT_USE_THIS_CARD:
						return ApiStrings.CameraError_CantUseThisCard;
					case CAMERA_ERROR.FORMAT_INTERNAL_MEM:
						return ApiStrings.CameraError_FormatInternalMem;
					case CAMERA_ERROR.FORMAT_CARD:
						return ApiStrings.CameraError_FormatCard;
					case CAMERA_ERROR.INTERNAL_MEM_ACCESS_FAIL:
						return ApiStrings.CameraError_InternalMemAccessFail;
					case CAMERA_ERROR.CARD_ACCESS_FAIL:
						return ApiStrings.CameraError_CardAccessFail;
					case CAMERA_ERROR.UNEXPECTED_ERROR:
						return ApiStrings.CameraError_UnexpectedError;
					case CAMERA_ERROR.BATTERY_CHARGE_FAIL:
						return ApiStrings.CameraError_BatteryChargeFail;
					case CAMERA_ERROR.HIGH_TEMPERATURE:
						return ApiStrings.CameraError_HighTemperature;
					case CAMERA_ERROR.BATTERY_HIGH_TEMPERATURE:
						return ApiStrings.CameraError_BatteryHighTemperature;
					case CAMERA_ERROR.UNKNOWN:
						return ApiStrings.CameraError_Unknown;
					default:
						return ApiStrings.Invalid_Value;
				}
			}
		}
	}

	public enum CAMERA_ERROR : uint
	{
		/// <summary> OK </summary>
		NONE = 0x0000000,
		/// <summary> Insufficient memory </summary>
		NO_MEMORY = 0x00000001,
		/// <summary> Error while writing </summary>
		WRITING_DATA = 0x00000002,
		/// <summary> File number exceeded the limit </summary>
		FILE_NUMBER_OVER = 0x00000004,
		/// <summary> Date time is not being set </summary>
		NO_DATE_SETTING = 0x00000008,
		/// <summary> Needs a compass calibration </summary>
		COMPASS_CALIBRATION = 0x00000010,
		/// <summary> SD card in not set </summary>
		CARD_DETECT_FAIL = 0x00000100,
		/// <summary> Hardware problem </summary>
		CAPTURE_HW_FAILED = 0x00400000,
		/// <summary> Media format error </summary>
		CANT_USE_THIS_CARD = 0x01000000,
		/// <summary> Internal memory format error </summary>
		FORMAT_INTERNAL_MEM = 0x02000000,
		/// <summary> SD card memory format error </summary>
		FORMAT_CARD = 0x04000000,
		/// <summary> Could not access internal memory </summary>
		INTERNAL_MEM_ACCESS_FAIL = 0x08000000,
		/// <summary> Could not access SD memory card </summary>
		CARD_ACCESS_FAIL = 0x10000000,
		/// <summary> Unexpected error </summary>
		UNEXPECTED_ERROR = 0x20000000,
		/// <summary> Battery charge failure </summary>
		BATTERY_CHARGE_FAIL = 0x40000000,
		/// <summary> High board temperature  </summary>
		HIGH_TEMPERATURE = 0x80000000,
		/// <summary> High battery temperature </summary>
		BATTERY_HIGH_TEMPERATURE = 0x00200000,
		/// <summary> UNKNOWN </summary>
		UNKNOWN = 0xFFFFFFFF
	}
}
