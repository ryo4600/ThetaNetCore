using System;
using System.Collections.Generic;
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
		public string[] CameraError { get; set; }

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
	}
}
