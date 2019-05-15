using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class ThetaResponse<T>
	{
		[DataMember(Name ="name")]
		public String Name { get; set; }

		[DataMember(Name ="state")]
		public String State { get; set; }

		[DataMember(Name = "results")]
		public T Results { get; set; }

		[DataMember(Name = "id", IsRequired =false)]
		public String Id { get; set; }

		[DataMember(Name ="progress", IsRequired =false)]
		public T Progress { get; set; }
	}
}
