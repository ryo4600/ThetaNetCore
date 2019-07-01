using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class SessionParam
	{
		[DataMember(Name = "sessionId")]
		public String SessionId { get; set; }
	}
}
