using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageRecognition.Recognizer
{
	public class ColorDouble
	{
		public double R, G, B;

		public static implicit operator Color(ColorDouble c)
		{
			return Color.FromArgb((int)c.R, (int)c.G, (int)c.B);
		}
		public static implicit operator ColorDouble(Color c)
		{
			return new ColorDouble { R = c.R, G = c.G, B = c.B };
		}

		public static ColorDouble operator+(ColorDouble x, Color y)
		{
			return new ColorDouble { R = x.R + y.R, G = x.R + y.R, B = x.B + y.B };
		}

		public static ColorDouble operator +(ColorDouble x, ColorDouble y)
		{
			return new ColorDouble { R = x.R + y.R, G = x.G + y.G, B = x.B + y.B };
		}

		public static ColorDouble operator /(ColorDouble x, double value)
		{
			if (value < 0)
				throw new DivideByZeroException();
			return new ColorDouble { R = x.R / value, G = x.G / value, B = x.B / value };
		}

		public override string ToString()
		{
			return string.Format("{{R={0}, G={1}, B={2}}}", R, G, B);
		}
	}

	public class ColorCounted
	{
		public Color Color;
		public int Count;

		public override string ToString()
		{
			return string.Format("{0} ({1})", Color, Count);
		}
	}

	public enum LineScanDirection
	{
		LeftToRight,
		RightToLeft,
		TopToBottom,
		BottomToTop
	}

	public class RecognizerTools
	{
		public int ColorDiff(Color x, Color y)
		{
			return Math.Abs(x.R - y.R) + Math.Abs(x.G - y.G) + Math.Abs(x.B - y.B);
		}

		public double ColorDiff(ColorDouble x, Color y)
		{
			return Math.Abs(x.R - y.R) + Math.Abs(x.G - y.G) + Math.Abs(x.B - y.B);
		}

		public double ColorDiff(Color x, ColorDouble y)
		{
			return Math.Abs(x.R - y.R) + Math.Abs(x.G - y.G) + Math.Abs(x.B - y.B);
		}

		public double ColorDiff(ColorDouble x, ColorDouble y)
		{
			return Math.Abs(x.R - y.R) + Math.Abs(x.G - y.G) + Math.Abs(x.B - y.B);
		}

		public bool TryGetAverageColor(Bitmap image, Rectangle bounds, int ColorThreshold, out ColorDouble averageColor)
		{
			averageColor = new ColorDouble();
			var count = 0;
			for (var x = bounds.Left; x < bounds.Right; x++)
					for (var y = bounds.Top; y < bounds.Bottom; y++)
				{
					var color = image.GetPixel(x, y);
					if (count > 0 && ColorDiff(averageColor / count, color) > ColorThreshold)
						return false;
					averageColor += color;
					count++;
				}
			averageColor = averageColor / count;
			return true;
		}

		public bool TryLineScan<T>(int lineStart, int lineEnd, int linePixelStart, int linePixelEnd,
				Func<int, int, Color> getPixel, Func<T, Color, T> lineAggregrate, Func<T, bool> lineScanStop, out int line, T aggregateInit = default(T))
		{
			for (var x = lineStart; x < lineEnd; x++)
			{
				T lineValue = aggregateInit;
				for (var y = linePixelStart; y < linePixelEnd; y++)
					lineValue = lineAggregrate(lineValue, getPixel(x, y));
				if (lineScanStop(lineValue))
				{
					line = x;
					return true;
				}
			}
			line = lineEnd;
			return false;
		}

		public bool TryLineScan<T>(Bitmap image, Rectangle bounds, LineScanDirection direction,
				Func<T, Color, T> lineAggregrate, Func<T, bool> lineScanStop, out int line, T aggregateInit = default(T))
		{
			switch (direction)
			{
				case LineScanDirection.LeftToRight:
					return TryLineScan(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom,
						(x, y) => image.GetPixel(x, y), lineAggregrate, lineScanStop, out line, aggregateInit);
				case LineScanDirection.RightToLeft:
					if (TryLineScan(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom,
						(x, y) => image.GetPixel(bounds.Right - (x - bounds.Left) - 1, y), lineAggregrate, lineScanStop, out line, aggregateInit))
					{
						line = bounds.Right - (line - bounds.Left) - 1;
						return true;
					}
					else
					{
						line = bounds.Left;
						return false;
					}
				case LineScanDirection.TopToBottom:
					return TryLineScan(bounds.Top, bounds.Bottom, bounds.Left, bounds.Right,
						(x, y) => image.GetPixel(y, x), lineAggregrate, lineScanStop, out line, aggregateInit);
				case LineScanDirection.BottomToTop:
					if (TryLineScan(bounds.Top, bounds.Bottom, bounds.Left, bounds.Right,
						(x, y) => image.GetPixel(y, bounds.Bottom - (x - bounds.Top) - 1), lineAggregrate, lineScanStop, out line, aggregateInit))
					{
						line = bounds.Bottom - (line - bounds.Top) - 1;
						return true;
					}
					else
					{
						line = bounds.Top;
						return false;
					}
				default:
					line = 0;
					return false;
			}
		}

		public List<ColorCounted> GetImageColors(Bitmap image, int ColorThreshold)
		{
			var colors = new List<ColorCounted>();
			for (var x = 0; x < image.Width; x++)
					for (var y = 0; y < image.Height; y++)
				{
					var pixel = image.GetPixel(x, y);
					var colorFound = false;
					foreach (var color in colors)
						if (ColorDiff(pixel, color.Color) < ColorThreshold)
						{
							color.Count += 1;
							colorFound = true;
							break;
						}
					if (!colorFound)
						colors.Add(new ColorCounted { Color = pixel, Count = 1 });
				}
			return colors;
		}

		public double GetColorPercentOnCircle(Bitmap image, ColorDouble color, Point center, double radius, int colorThreshold)
		{
			var totalCount = 0;
			var colorCount = 0;
			for (var angle = 0d; angle < 2 * Math.PI; angle += 1 / radius)
			{
				var p = new Point((int)(center.X + Math.Cos(angle) * radius), (int)(center.Y + Math.Sin(angle) * radius));
				if (p.X < 0 || p.Y < 0 || p.X >= image.Width || p.Y >= image.Height)
					continue;
				var pixel = image.GetPixel(p.X, p.Y);
				if (ColorDiff(color, pixel) < colorThreshold)
					colorCount++;
				totalCount++;
			}
			return colorCount * 1.0 / totalCount;
		}
	}
}
