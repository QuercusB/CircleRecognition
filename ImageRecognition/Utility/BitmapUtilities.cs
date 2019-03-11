using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ImageRecognition.Utility
{
	public static class BitmapUtilities
	{
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		public static BitmapSource ToBitmapImage(this Bitmap bitmap)
		{
			IntPtr hBitmap = bitmap.GetHbitmap();
			BitmapSource retval;

			try
			{
				retval = Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(hBitmap);
			}

			return retval;
		}
	}
}
