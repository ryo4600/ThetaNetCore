using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class ErrorResponse
	{
		[DataMember(Name ="state")]
		public String State { get; set; }

		[DataMember(Name ="error")]
		public ErrorDetail Error { get; set; }
	}

	[DataContract]
	public class ErrorDetail
	{
		[DataMember(Name ="code")]
		public String Code { get; set; }

		[DataMember(Name ="message")]
		public String Message { get; set; }
	}
}
