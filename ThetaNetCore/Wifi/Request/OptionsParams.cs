using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class GetOptionParams
	{
		[DataMember(Name = "optionNames", IsRequired =false)]
		public String[] Options { get; set; }
	}

	[DataContract]
	public class SetOptionParams
	{
		[DataMember(Name = "sessionId", IsRequired = false, EmitDefaultValue =false)]
		public String SessionId { get; set; }

		[DataMember(Name = "options", IsRequired = false)]
		public OptionValues Options { get; set; }
	}
}
