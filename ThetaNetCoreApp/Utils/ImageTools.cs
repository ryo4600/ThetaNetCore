using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ThetaNetCoreApp.Utils
{
	static class ImageTools
	{
		/// <summary>
		/// Load image from file. <br /> This one is fast.
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		static public BitmapSource LoadThumbnail(String filepath, int pixelHeight = 90)
		{
			BitmapSource ret = null;
			BitmapMetadata meta = null;

			try
			{
				BitmapFrame frame = BitmapFrame.Create(
					new Uri(filepath),
					BitmapCreateOptions.DelayCreation,
					BitmapCacheOption.None);

				if (frame.Thumbnail == null)
				{
					BitmapImage image = new BitmapImage();
					image.DecodePixelHeight = pixelHeight;
					image.BeginInit();
					image.UriSource = new Uri(filepath);
					image.CacheOption = BitmapCacheOption.None;
					image.CreateOptions = BitmapCreateOptions.DelayCreation;
					image.EndInit();

					if (image.CanFreeze)
						image.Freeze();

					ret = image;
				}
				else
				{
					meta = frame.Metadata as BitmapMetadata;
					ret = frame.Thumbnail;
				}

			}
			catch (Exception)
			{
			}

			return ret;
		}

	}
}
