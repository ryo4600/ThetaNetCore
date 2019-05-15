using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class InfoResponse
	{
		/// <summary>
		/// Manufacturer
		/// </summary>
		[DataMember(Name = "manufacturer")]
		public String Manufacture { get; set; }

		/// <summary>
		/// Model
		/// </summary>
		[DataMember(Name = "model")]
		public String Model { get; set; }

		/// <summary>
		/// Serial No
		/// </summary>
		[DataMember(Name = "serialNumber")]
		public String SerialNumber { get; set; }

		/// <summary>
		/// Firmware Version
		/// </summary>
		[DataMember(Name = "firmwareVersion")]
		public String FirmwareVersion { get; set; }

		/// <summary>
		/// URL for the theta support
		/// </summary>
		[DataMember(Name = "supportUrl")]
		public String SupportUrl { get; set; }

		/// <summary>
		/// Has GPS ?
		/// </summary>
		[DataMember(Name = "gps")]
		public bool Gps { get; set; }

		/// <summary>
		/// Has Gyroscope? 
		/// </summary>
		[DataMember(Name = "gyro")]
		public bool Gyro { get; set; }

		/// <summary>
		/// Up time in sec.
		/// </summary>
		[DataMember(Name = "uptime")]
		public int Uptime { get; set; }

		/// <summary>
		/// List of supported APIs
		/// </summary>
		[DataMember(Name = "api")]
		public String[] Api { get; set; }

		/// <summary>
		/// End points information
		/// </summary>
		[DataMember(Name ="endpoints")]
		public Endpoints EndPoints { get; set; }

		/// <summary>
		/// Supported API Level <br />
		/// 1: v2.0, 2:v2.1
		/// </summary>
		[DataMember(Name = "apiLevel")]
		public int[] ApiLevel { get; set; }
	}

	[DataContract]
	public class Endpoints
	{
		/// <summary>
		/// Port no. used for API execution.
		/// </summary>
		[DataMember(Name ="httpPort")]
		public int HttpPort { get; set; }

		/// <summary>
		/// Port no. used for "CheckForUpdate"
		/// </summary>
		[DataMember(Name = "httpUpdatesPort")]
		public int HttpUpdatesPort { get; set; }
	}
}
