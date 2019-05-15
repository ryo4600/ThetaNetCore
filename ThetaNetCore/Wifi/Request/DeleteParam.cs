using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class DeleteParam
	{
		/// <summary>
		/// File urls to delete or set followings <br />
		/// "all" : delte all files <br />
		/// "image" : delete all images <br />
		/// "video" : delete all movies
		/// </summary>
		[DataMember(Name = "fileUrls")]
		public String[] FileUrls { get; set; }
	}

}
