using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class SetPluginParam
	{
		[DataMember(Name = "packageName")]
		public String PackageName { get; set; }

		[DataMember(Name = "boot")]
		public bool Boot { get; set; } = true;
	}
}
