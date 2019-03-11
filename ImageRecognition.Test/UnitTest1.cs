using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageRecognition.Test
{
	[TestClass]
	[Ignore("Explicit")]
	public class UnitTest1
	{
		private string[] samples;
		private string outputFolder;

		public UnitTest1()
		{
			var samplesFolder = Path.Combine(Environment.CurrentDirectory, "samples");
			outputFolder = Path.Combine(Environment.CurrentDirectory, "samples-out");
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);
			samples = Directory.GetFiles(samplesFolder, "*.jpg");
			foreach (var sample in samples)
				Console.WriteLine(sample);
		}

		public int findMedian(double[] data)
		{
			var top = -1;
			var bottom = data.Length;
			var topTotal = 0d;
			var bottomTotal = 0d;
			while (top < bottom)
			{
				if (topTotal <= bottomTotal)
					topTotal += data[++top];
				else
					bottomTotal += data[--bottom];
			}
			return top;
		}

		[TestMethod]
		public void TestMethod1()
		{
			foreach (var samplePath in samples) {
				//var samplePath = samples[0];
				var sampleName = Path.GetFileName(samplePath);
				var image = Image.FromFile(samplePath) as Bitmap;
				Console.WriteLine("{0}, Width: {1}, Height: {2}", sampleName, image.Width, image.Height);
				var vBrightness = new double[image.Height];
				for (var y = 0; y < image.Height; y++)
				{
					var brightness = 0d;
					for (var x = 0; x < image.Width; x++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.9d)
							continue; // ignoring - it is white text;
						brightness += pixelBrightness;
					}
					brightness = brightness / image.Width;
					vBrightness[y] = brightness;
				}
				// searching for median
				Console.WriteLine("Median (y): {0}", findMedian(vBrightness));

				var hBrightness = new double[image.Width];
				for (var x = 0; x < image.Width; x++)
				{
					var brightness = 0d;
					for (var y = 0; y < image.Height; y++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.9d)
							continue; // ignoring - it is white text;
						brightness += pixelBrightness;
					}
					brightness = brightness / image.Height;
					hBrightness[x] = brightness;
				}
				// searching for median
				Console.WriteLine("Median (x): {0}", findMedian(hBrightness));

				var pixelCount = 0;
				for (var x = 0; x < image.Width; x++)
				{
					for (var y = 0; y < image.Height; y++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.35 && pixelBrightness < 0.9d)
							pixelCount++;
					}
				}
				var radius = Math.Sqrt((double)pixelCount / Math.PI);
				Console.WriteLine("PixelCount: {0}. Radius: {1}", pixelCount, radius);

				var topLine = 0;
				for (var y = 0; y < image.Height; y++)
				{
					var brightPixelCount = 0;
					for (var x = 0; x < image.Width; x++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.3d && pixelBrightness < 0.9d)
						{
							brightPixelCount++;
							if (brightPixelCount * 20 > image.Width)
								break;
						}
					}
					if (brightPixelCount * 20 > image.Width)
					{
						topLine = y;
						break;
					}
				}
				var bottomLine = 0;
				for (var y = image.Height - 1; y >= 0; y--)
				{
					var brightPixelCount = 0;
					for (var x = 0; x < image.Width; x++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.3d && pixelBrightness < 0.9d)
						{
							brightPixelCount++;
							if (brightPixelCount * 20 > image.Width)
								break;
						}
					}
					if (brightPixelCount * 20 > image.Width)
					{
						bottomLine = y;
						break;
					}
				}

				Console.WriteLine("Top: {0}, Bottom: {1}, Center: {2}, Radius: {3}",
						topLine, bottomLine, (topLine + bottomLine) / 2, (bottomLine - topLine) / 2);

				var leftLine = 0;
				for (var x = 0; x < image.Width; x++)
				{
					var brightPixelCount = 0;
					for (var y = 0; y < image.Height; y++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.3d && pixelBrightness < 0.9d)
						{
							brightPixelCount++;
							if (brightPixelCount * 20 > image.Height)
								break;
						}
					}
					if (brightPixelCount * 20 > image.Height)
					{
						leftLine = x;
						break;
					}
				}
				var rightLine = 0;
				for (var x = image.Width - 1; x >= 0; x--)
				{
					var brightPixelCount = 0;
					for (var y = 0; y < image.Height; y++)
					{
						var pixelBrightness = image.GetPixel(x, y).GetBrightness();
						if (pixelBrightness > 0.3d && pixelBrightness < 0.9d)
						{
							brightPixelCount++;
							if (brightPixelCount * 20 > image.Height)
								break;
						}
					}
					if (brightPixelCount * 20 > image.Height)
					{
						rightLine = x;
						break;
					}
				}

				Console.WriteLine("Left: {0}, Right: {1}, Center: {2}, Radius: {3}",
						leftLine, rightLine, (leftLine + rightLine) / 2, (rightLine - leftLine) / 2);

				var center = new Point((leftLine + rightLine) / 2, (topLine + bottomLine) / 2);
				var r = (bottomLine - topLine + rightLine - leftLine) / 4.0;
				using (var g = Graphics.FromImage(image))
				{
					g.DrawLine(Pens.White, 0, center.Y, image.Width - 1, center.Y);
					g.DrawLine(Pens.White, center.X, 0, center.X, image.Height - 1);


					g.DrawEllipse(Pens.White, (int)(center.X - r), (int)(center.Y - r), (int)(2 * r), (int)(2 * r));
				}
				image.Save(Path.Combine(outputFolder, sampleName), ImageFormat.Jpeg);
                break;
			}

			Assert.AreEqual(1, 2);
		}
	}
}
