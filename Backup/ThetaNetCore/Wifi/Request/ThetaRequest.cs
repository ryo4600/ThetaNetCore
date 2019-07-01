using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class ThetaRequest<T>
	{
		[DataMember(Name ="name")]
		public String Name { get; set; }

		[DataMember(Name ="parameters")]
		public T Parameters { get; set; }
	}

}
