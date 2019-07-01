using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class ListFilesResponse
	{
		[DataMember(Name = "totalEntries")]
		public int TotalEntries { get; set; }

		[DataMember(Name = "entries")]
		public FileEntry[] Entries { get; set; }
	}

	[DataContract]
	public class FileEntry
	{
		/// <summary>
		/// File Name
		/// </summary>
		[DataMember(Name = "name")]
		public String Name { get; set; }

		/// <summary>
		/// File URL
		/// </summary>
		[DataMember(Name = "fileUrl")]
		public String FileUrl { get; set; }

		/// <summary>
		/// File size (byte)
		/// </summary>
		[DataMember(Name = "size")]
		public int Size { get; set; }

		/// <summary>
		/// Time that a file with a time zone is created or updated <br />
		/// This can be acquired when "_detail" is true
		/// </summary>
		[DataMember(Name = "dateTimeZone")]
		public String DateTimeZone { get; set; }

		/// <summary>
		/// Time that a file is created or updated <br />
		/// Local time <br />
		/// This can be acquired when "_detail" is false
		/// </summary>
		[DataMember(Name = "dateTime", IsRequired = false)]
		public String DateTime { get; set; }

		/// <summary>
		/// Latitude <br />
		/// This can be acquired for an image shot when GPS is enabled
		/// </summary>
		[DataMember(Name = "lat")]
		public double Latitude { get; set; }

		/// <summary>
		/// Longitude  <br />
		/// This can be acquired for an image shot when GPS is enabled
		/// </summary>
		[DataMember(Name = "lng")]
		public double Longitude { get; set; }

		/// <summary>
		/// Width of the image (px)
		/// </summary>
		[DataMember(Name = "width")]
		public int Width { get; set; }

		/// <summary>
		/// Height of the image (px)
		/// </summary>
		[DataMember(Name = "height")]
		public int Height { get; set; }

		/// <summary>
		/// Thumbnail<br />
		/// Encoded in base64 <br />
		/// This can be acquired when "maxThumbSize" is enabled.
		///  </summary>
		[DataMember(Name = "thumbnail")]
		public String Thumbnail { get; set; }

		/// <summary>
		/// Decoded image data from EncodedThumbnail
		/// </summary>
		[IgnoreDataMember]
		public byte[] ThumbnailData
		{
			get
			{
				if (Thumbnail == null)
					return null;
				return Convert.FromBase64String(Thumbnail);
			}
		}

		///// <summary>
		///// Thumbnail file size (byte) <br />
		///// This can be acquired when "maxThumbSize" is enabled.
		///// </summary>
		//[DataMember(Name = "_thumbSize")]
		//public int ThumbSize { get; set; }

		///// <summary>
		///// Group ID of still images shot using interval shooting <br />
		///// This can be acquired when the file is shoot using interval shooting
		/////  </summary>
		//[DataMember(Name = "_intervalCaptureGroupId")]
		//public String IntervalCaptureGroupId { get; set; }

		///// <summary>
		///// Video recording time (sec.) <br />
		///// This can be acquired when the file is video.
		///// </summary>
		//[DataMember(Name = "_recordTime")]
		//public int RecordTime { get; set; }

		///// <summary>
		///// Whether or not the image processing completed file
		///// </summary>
		//[DataMember(Name = "isProcessed")]
		//public bool IsProcessed { get; set; }

		///// <summary>
		///// File URL in the middle of the image processing
		///// </summary>
		//[DataMember(Name = "previewUrl")]
		//public String PreviewUrl { get; set; }


	}
}
