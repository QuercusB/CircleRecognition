using ImageRecognition.Recognizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace ImageRecognition.Test.Recognizer
{
	[TestClass]
	public class RecognizerToolsTest
	{
		private RecognizerTools recognizerTools = new RecognizerTools();

		private static void AssertColorsEqual(Color expected, Color actual)
		{
			Assert.AreEqual(
				expected.ToArgb(),
				actual.ToArgb(),
				string.Format("Expected color <R: {0}, G: {1}, B: {2}> but got <R: {3}, G: {4}, B: {5}>",
					expected.R, expected.G, expected.B, actual.R, actual.G, actual.B));
		}

		private static void AssertColorsEqual(Color expected, ColorDouble actual)
		{
			Assert.IsTrue(
				Math.Abs(expected.R - actual.R) < 0.0001 &&
				Math.Abs(expected.G - actual.G) < 0.0001 &&
				Math.Abs(expected.B - actual.B) < 0.0001,
				string.Format("Expected color <R: {0}, G: {1}, B: {2}> but got <R: {3}, G: {4}, B: {5}>",
					expected.R, expected.G, expected.B, actual.R, actual.G, actual.B));
		}

		private static void AssertColorsEqual(ColorDouble expected, ColorDouble actual)
		{
			Assert.IsTrue(
				Math.Abs(expected.R - actual.R) < 0.0001 &&
				Math.Abs(expected.G - actual.G) < 0.0001 &&
				Math.Abs(expected.B - actual.B) < 0.0001,
				string.Format("Expected color <R: {0}, G: {1}, B: {2}> but got <R: {3}, G: {4}, B: {5}>",
					expected.R, expected.G, expected.B, actual.R, actual.G, actual.B));
		}

		[TestMethod]
		public void ColorDiff_ShouldCalculateDifferenceBetweenColors()
		{
			var x = Color.FromArgb(100, 100, 100);
			var y = Color.FromArgb(120, 105, 110);
			Assert.AreEqual(35, recognizerTools.ColorDiff(x, y));
		}

		[TestMethod]
		public void ColorDiff_ShouldCalculateDifferenceBetweenColorsUsingAbsoluteDiff()
		{
			var x = Color.FromArgb(100, 100, 100);
			var y = Color.FromArgb(120, 105, 90);
			Assert.AreEqual(35, recognizerTools.ColorDiff(x, y));
		}

		[TestMethod]
		public void GetAverageColor_ShouldCalculateAverageColorForGivenBounds()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.FillRectangle(Brushes.Yellow, new Rectangle(0, 0, 20, 20));
				Console.WriteLine("{0}", bitmap.GetPixel(10, 10));
			}
			ColorDouble averageColor;
			var result = recognizerTools.TryGetAverageColor(bitmap, new Rectangle(0, 0, 20, 20), 1, out averageColor);
			Assert.IsTrue(result);
			AssertColorsEqual(Color.Yellow, averageColor);
		}

		[TestMethod]
		public void GetAverageColor_ShouldAllowDifferentColorInThreshold()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			bitmap.SetPixel(5, 5, Color.FromArgb(100, 100, 100));
			bitmap.SetPixel(5, 6, Color.FromArgb(104, 100, 100));
			bitmap.SetPixel(6, 5, Color.FromArgb(100, 104, 100));
			bitmap.SetPixel(6, 6, Color.FromArgb(100, 100, 104));
			ColorDouble averageColor;
			var result = recognizerTools.TryGetAverageColor(bitmap, new Rectangle(5, 5, 2, 2), 12, out averageColor);
			Assert.IsTrue(result);
			AssertColorsEqual(new ColorDouble { R = 101, G = 101, B = 101 }, averageColor);
		}

		[TestMethod]
		public void GetAverageColor_ShouldReturnFalseIfThresholdIsViolated()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			bitmap.SetPixel(5, 5, Color.FromArgb(100, 100, 100));
			bitmap.SetPixel(5, 6, Color.FromArgb(104, 100, 100));
			bitmap.SetPixel(6, 5, Color.FromArgb(100, 114, 100)); // 114 is out of threshold
			bitmap.SetPixel(6, 6, Color.FromArgb(100, 100, 104));
			ColorDouble averageColor;
			var result = recognizerTools.TryGetAverageColor(bitmap, new Rectangle(5, 5, 2, 2), 12, out averageColor);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void LineScan_CanScanFromLeftToRight()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(bitmap))
				g.FillEllipse(Brushes.LightGray, new Rectangle(5, 8, 10, 10));
			int line;
			var result = recognizerTools.TryLineScan<int>(bitmap, new Rectangle(0, 0, 20, 20),
					LineScanDirection.LeftToRight,
					(v, color) => v + ((color.R == Color.LightGray.R) ? 1 : 0),
					(v) => v > 5,
					out line);
			Assert.IsTrue(result);
			Assert.AreEqual(7, line);
		}
		[TestMethod]

		public void LineScan_CanScanromRightToLeft()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(bitmap))
				g.FillEllipse(Brushes.LightGray, new Rectangle(5, 8, 10, 10));
			int line;
			var result = recognizerTools.TryLineScan<int>(bitmap, new Rectangle(0, 0, 20, 20),
					LineScanDirection.RightToLeft,
					(v, color) => v + ((color.R == Color.LightGray.R) ? 1 : 0),
					(v) => v > 5,
					out line);
			Assert.IsTrue(result);
			Assert.AreEqual(13, line);
		}

		[TestMethod]
		public void LineScan_CanScanFromTopToBottom()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(bitmap))
				g.FillEllipse(Brushes.LightGray, new Rectangle(5, 8, 10, 10));
			int line;
			var result = recognizerTools.TryLineScan<int>(bitmap, new Rectangle(0, 0, 20, 20),
					LineScanDirection.TopToBottom,
					(v, color) => v + ((color.R == Color.LightGray.R) ? 1 : 0),
					(v) => v > 5,
					out line);
			Assert.IsTrue(result);
			Assert.AreEqual(10, line);
		}

		[TestMethod]
		public void LineScan_CanScanFromBottomToTop()
		{
			var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(bitmap))
				g.FillEllipse(Brushes.LightGray, new Rectangle(5, 8, 10, 10));
			int line;
			var result = recognizerTools.TryLineScan<int>(bitmap, new Rectangle(0, 0, 20, 20),
					LineScanDirection.BottomToTop,
					(v, color) => v + ((color.R == Color.LightGray.R) ? 1 : 0),
					(v) => v > 5,
					out line);
			Assert.IsTrue(result);
			Assert.AreEqual(16, line);
		}

		[TestMethod]
		public void GetImageColors_FindsAllColorsAndCountsThem()
		{
			var image = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(image))
			{
				g.FillRectangle(Brushes.Blue, 0, 0, 4, 4);
				g.FillRectangle(Brushes.Yellow, 10, 10, 2, 3);
			}
			var result = recognizerTools.GetImageColors(image, 1);
			result.Sort((x, y) => y.Count - x.Count);

			Console.WriteLine(result.Aggregate("", (s, x) => s + (s.Length > 0 ? ", " : "") + x));

			Assert.AreEqual(3, result.Count);
			AssertColorsEqual(Color.Black, result[0].Color);
			Assert.AreEqual(20 * 20 - 4 * 4 - 2 * 3, result[0].Count);
			AssertColorsEqual(Color.Blue, result[1].Color);
			Assert.AreEqual(4 * 4, result[1].Count);
			AssertColorsEqual(Color.Yellow, result[2].Color);
			Assert.AreEqual(2 * 3, result[2].Count);
		}

		[TestMethod]
		public void GetImageColors_AggregateSameColorsWithinThreshold()
		{
			var image = new Bitmap(20, 20, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(image))
			{
				g.FillRectangle(Brushes.Blue, 0, 0, 4, 4);
				g.FillRectangle(Brushes.Yellow, 10, 10, 2, 3);

				// making a bit worse image
				// blue rect
				image.SetPixel(2, 2, Color.FromArgb(Color.Blue.R + 1, Color.Blue.G, Color.Blue.B - 1));
				image.SetPixel(1, 3, Color.FromArgb(Color.Blue.R + 2, Color.Blue.G + 1, Color.Blue.B));
				image.SetPixel(0, 1, Color.FromArgb(Color.Blue.R, Color.Blue.G + 1, Color.Blue.B - 1));

				// yellow rect
				image.SetPixel(10, 12, Color.FromArgb(Color.Yellow.R - 1, Color.Yellow.G, Color.Yellow.B + 1));
				image.SetPixel(11, 11, Color.FromArgb(Color.Yellow.R - 2, Color.Yellow.G - 1, Color.Yellow.B));
			}
			var result = recognizerTools.GetImageColors(image, 5);
			result.Sort((x, y) => y.Count - x.Count);

			Console.WriteLine(result.Aggregate("", (s, x) => s + (s.Length > 0 ? ", " : "") + x));

			Assert.AreEqual(3, result.Count);
			AssertColorsEqual(Color.Black, result[0].Color);
			Assert.AreEqual(20 * 20 - 4 * 4 - 2 * 3, result[0].Count);
			AssertColorsEqual(Color.Blue, result[1].Color);
			Assert.AreEqual(4 * 4, result[1].Count);
			AssertColorsEqual(Color.Yellow, result[2].Color);
			Assert.AreEqual(2 * 3, result[2].Count);
		}
	}
}
