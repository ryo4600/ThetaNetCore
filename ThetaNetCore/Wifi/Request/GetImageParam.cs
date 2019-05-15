using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class GetImageParam
	{
		[DataMember(Name ="fileUri")]
		public String FileUri { get; set; }
	}

	/*
	POST /osc/commands/execute
	{
		"name": "camera.getImage",
		"parameters": {
			"fileUri": "100RICOH/R0010005.JPG"
		}
	}
	*/
}
