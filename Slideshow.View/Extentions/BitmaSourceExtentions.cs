using System.IO;
using System.Windows.Media.Imaging;

namespace Slideshow.View.Extentions
{
	public static class BitmaSourceExtentions
	{
		public static BitmapImage ToBitmapImage(this BitmapSource bitmapSource)
		{
			var encoder = new JpegBitmapEncoder();
			using (var memoryStream = new MemoryStream())
			{
				var bImg = new BitmapImage();

				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				encoder.Save(memoryStream);

				bImg.BeginInit();
				bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
				bImg.EndInit();
				bImg.Freeze();

				return bImg;
			}
		}
	}
}
