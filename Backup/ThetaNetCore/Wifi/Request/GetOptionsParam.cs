using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class GetOptionsParam
	{
		[IgnoreDataMember()]
		public bool Aperture { get; set; }

		[IgnoreDataMember()]
		public bool CaptureInterval { get; set; }

		[IgnoreDataMember()]
		public bool CaptureMode { get; set; }

		[IgnoreDataMember()]
		public bool CaptureNumber { get; set; }

		[IgnoreDataMember()]
		public bool ClientVersion { get; set; }

		[IgnoreDataMember()]
		public bool DateTimeZone { get; set; }

		[IgnoreDataMember()]
		public bool ExposureCompensation { get; set; }

		[IgnoreDataMember()]
		public bool ExposureDelay { get; set; }

		[IgnoreDataMember()]
		public bool ExposureProgram { get; set; }

		[IgnoreDataMember()]
		public bool FileFormat { get; set; }

		[IgnoreDataMember()]
		public bool Filter { get; set; }

		[IgnoreDataMember()]
		public bool GpsInfo { get; set; }

		[IgnoreDataMember()]
		public bool HDMIReso { get; set; }

		[IgnoreDataMember()]
		public bool Iso { get; set; }

		[IgnoreDataMember()]
		public bool LatestEnabledExposureDelayTime { get; set; }

		[IgnoreDataMember()]
		public bool OffDelay { get; set; }

		[IgnoreDataMember()]
		public bool PreviewFormat { get; set; }

		[IgnoreDataMember()]
		public bool RemainingPictures { get; set; }

		[IgnoreDataMember()]
		public bool RemainingSpace { get; set; }

		[IgnoreDataMember()]
		public bool RemainingVideoSeconds { get; set; }

		[IgnoreDataMember()]
		public bool ShutterSpeed { get; set; }

		[IgnoreDataMember()]
		public bool ShutterVolume { get; set; }

		[IgnoreDataMember()]
		public bool SleepDelay { get; set; }

		[IgnoreDataMember()]
		public bool TotalSpace { get; set; }

		[IgnoreDataMember()]
		public bool WhiteBalance { get; set; }

		[IgnoreDataMember()]
		public bool WlanChannel { get; set; }

		[DataMember(Name = "optionNames")]
		public String[] OptionNames
		{
			get
			{
				return GatherRequests();
			}
		}

		/// <summary>
		/// Collect properties to get, which are set to "true" for equivalant flags.
		/// </summary>
		/// <returns></returns>
		private string[] GatherRequests()
		{
			var request = new List<String>();
			if( Aperture)
				request.Add("aperture");
			if( CaptureInterval)
				request.Add("captureInterval");
			if( CaptureMode)
				request.Add("captureMode");
			if( CaptureNumber)
				request.Add("captureNumber");
			if( ClientVersion)
				request.Add("clientVersion");
			if(DateTimeZone)
				request.Add("dateTimeZone");
			if(ExposureCompensation)
				request.Add("exposureCompensation");
			if(ExposureDelay)
				request.Add("exposureDelay");
			if(ExposureProgram)
				request.Add("exposureProgram");
			if(FileFormat)
				request.Add("fileFormat");
			if(Filter)
				request.Add("_filter");
			if(GpsInfo)
				request.Add("gpsInfo");
			if(HDMIReso)
				request.Add("_HDMIreso");
			if(Iso)
				request.Add("iso");
			if( LatestEnabledExposureDelayTime)
				request.Add("_latestEnabledExposureDelayTime");
			if(OffDelay)
				request.Add("offDelay");
			if(PreviewFormat)
				request.Add("previewFormat");
			if(RemainingPictures)
				request.Add("remainingPictures");
			if(RemainingSpace)
				request.Add("remainingSpace");
			if(RemainingVideoSeconds)
				request.Add("remainingVideoSeconds");
			if(ShutterSpeed)
				request.Add("shutterSpeed");
			if(ShutterVolume)
				request.Add("_shutterVolume");
			if(SleepDelay)
				request.Add("sleepDelay");
			if(TotalSpace)
				request.Add("totalSpace");
			if(WhiteBalance)
				request.Add("whiteBalance");
			if(WlanChannel)
				request.Add("_wlanChannel");

			return request.ToArray();
		}
	}

}
