using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class CheckForUpdateRequest
	{
		/// <summary>
		/// State ID
		/// </summary>
		[DataMember(Name = "stateFingerprint")]
		public String StateFingerprint { get; set; }
	}

	[DataContract]
	public class CheckForUpdateResponse : CheckForUpdateRequest
	{
	}
}
