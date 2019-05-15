using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class ListFilesParam
	{
		/// <summary>
		/// Type of files to be acquired.
		/// </summary>
		[IgnoreDataMember]
		public ThetaFileType FileType { get; set; }

		[DataMember(Name = "fileType")]
		public String FileTypeString
		{
			set { }
			get { return ThetaTools.ToString(this.FileType); }
		}

		/// <summary>
		/// (Optional) <br />
		/// Start position on the file list <br />
		/// If this parameter is larger than the number of total files, an empty list is acquired <br />
		/// Default is the top of list
		/// </summary>
		[DataMember(Name = "startPosition", EmitDefaultValue = false)]
		public int StartPosition { get; set; }

		/// <summary>
		/// The number of still images and video files to be acquired<br />
		/// Entries is acquired in available range when this parameter is larger <br />
		/// than the number of total files<br/>
		/// If thumbnails are acquired, only one file can be acquired
		/// </summary>
		[DataMember(Name = "entryCount")]
		public int EntryCount { get; set; }

		private int _maxThumbSize = 640;
		/// <summary>
		/// Maximum thumbnails size (px)<br/>
		/// Fixed value: 640<br/>
		/// If this parameter is 0 or null, the thumbnail is not acquired<br/>
		/// This should be specified regardless of "_detail"
		/// </summary>
		[DataMember(Name = "maxThumbSize")]
		public int MaxThumbSize
		{
			get { return _maxThumbSize; }
			set { _maxThumbSize = value; }
		}

		private bool _detail = true;
		/// <summary>
		/// (Optional) <br />
		/// Whether or not file details are acquired <br />
		/// Default is true <br />
		/// Only values that can be acquired when false is specified are "name", "fileUrl", "size", "dateTime"
		/// </summary>
		[DataMember(Name ="_detail")]
		public bool Detail
		{
			get { return _detail; }
			set { _detail = value; }
		}

		private SortOrder _order = SortOrder.Newest;
		[IgnoreDataMember]
		public SortOrder Order
		{
			get { return _order; }
			set { _order = value; }
		}

		/// <summary>
		/// (Optional) <br />
		/// Specify the sort order <br />
		/// newest(shooting date and time descending order)/ oldest(shooting date and time ascending order) <br />
		/// Default is newest
		/// </summary>
		[DataMember(Name="_sort")]
		public String OrderString
		{
			set { }
			get { return ThetaTools.ToString(this.Order); }
		}
	}

}
