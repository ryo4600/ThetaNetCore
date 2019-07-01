using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract(Name = "completion")]
	public class TakePictureResponse
	{
		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "state")]
		public String State { get; set; }

		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name ="progress")]
		public ProgressDetail Progress { get; set; }
	}

	[DataContract]
	public class ProgressDetail
	{
		[DataMember(Name ="completion")]
		public double Completion { get; set; }
	}

/*{
	name": "camera.takePicture",
    "state": "inProgress",
    "id": "1",
    "progress": {
        "completion": 0
    }
}*/

}
