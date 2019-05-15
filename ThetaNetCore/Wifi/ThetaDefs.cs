using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThetaNetCore
{
	public enum ThetaFileType { ALL, Image, Video };
	public enum SortOrder { Newest, Olderst };

	public static class ThetaTools
	{
		public static String ToString(ThetaFileType fileType)
		{
			switch (fileType)
			{
				case ThetaFileType.Image:
					return "image";
				case ThetaFileType.Video:
					return "video";
				case ThetaFileType.ALL:
				default:
					return "all";
			}
		}

		public static String ToString(SortOrder order)
		{
			switch (order)
			{
				case SortOrder.Olderst:
					return "oldest";
				case SortOrder.Newest:
				default:
					return "newest";
			}
		}
	}
}
