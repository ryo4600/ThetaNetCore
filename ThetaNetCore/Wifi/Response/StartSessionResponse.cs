using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class StartSessionResponse
	{
		[DataMember(Name = "sessionId")]
		public String SessionId { get; set; }

		[DataMember(Name = "timeout")]
		public int Timeout { get; set; }

	}
}