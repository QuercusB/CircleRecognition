using ImageRecognition.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageRecognition.Recognizer
{
	public class RecognizerResult
	{
		public Point OuterCircleCenter { get; set; }
		public double OuterCircleRadius { get; set; }
		public Point InnerCircleCenter { get; set; }
		public double InnerCircleRadius { get; set; }
	}

	public class Recognizer
	{
		public Recognizer()
		{
			Tools = new RecognizerTools();
		}

		public ILog Log { get; set; }

		public RecognizerTools Tools { get; set; }

		public RecognizerResult Run(Bitmap image)
		{
			var colorThreshold = 25;
			var needDispose = false;
			var multiplier = 1;
			var smallImage = image;
			while (smallImage.Width > 600 || smallImage.Height > 600)
			{
				Log?.Debug("Downsample image to {0} x {1} for faster initial search", smallImage.Width / 2, smallImage.Height / 2);
				var newImage = new Bitmap(smallImage, new Size(smallImage.Width / 2, smallImage.Height / 2));
				multiplier *= 2;
				if (needDispose)
					smallImage.Dispose();
				smallImage = newImage;
				needDispose = true;
			}
			try
			{
				var corners = new Rectangle[] {
					new Rectangle(0, 0, 10, 10),
					new Rectangle(smallImage.Width - 10, 0, 10, 10),
					new Rectangle(0, smallImage.Height - 10, 10, 10),
					new Rectangle(smallImage.Width - 10, smallImage.Height - 10, 10, 10)
				};
				var backColorFound = false;
				var backColor = new ColorDouble();
				foreach (var corner in corners)
				{
					Log?.Debug("Selecting background color as average of {0} rectangle", corner);
					if (Tools.TryGetAverageColor(smallImage, corner, colorThreshold, out backColor))
					{
						backColorFound = true;
						Log?.Info("Treating color {0} as background color", backColor);
						break;
					}
					else
						Log?.Debug("Color at {0} are mixed - trying other corner", corner);
				}
				if (!backColorFound)
				{
					Log?.Error("Failed to determing background color");
					return null;
				}
				var colors = Tools.GetImageColors(smallImage, colorThreshold);
				colors.Sort((x, y) => y.Count - x.Count);
				Log?.Debug("Retrieved all image colors: {0}",
					colors.Aggregate("", (s, c) => s + (s.Length > 0 ? ", " : "") + c));
				Log?.Debug("Taking the most used color that is not a background");
				var outerCircleColor = colors.First(c => Tools.ColorDiff(backColor, c.Color) >= colorThreshold).Color;
				Log?.Info("Outer circle color = {0}", outerCircleColor);

				Log?.Debug("Scanning for outer circle bounds a for lines, that have more than 5% of outer circle color");
				int outerCircleLeft, outerCircleRight, outerCircleTop, outerCircleBottom;

				if (!Tools.TryLineScan<int>(smallImage, new Rectangle(Point.Empty, smallImage.Size),
						LineScanDirection.LeftToRight,
						(count, color) => count + (Tools.ColorDiff(outerCircleColor, color) < colorThreshold ? 1 : 0),
						(count) => count > smallImage.Height / 20,
						out outerCircleLeft))
				{
					Log?.Error("Failed to find left border of the outer circle");
					return null;
				}
				if (!Tools.TryLineScan<int>(smallImage, new Rectangle(Point.Empty, smallImage.Size),
						LineScanDirection.RightToLeft,
						(count, color) => count + (Tools.ColorDiff(outerCircleColor, color) < colorThreshold ? 1 : 0),
						(count) => count > smallImage.Height / 20,
						out outerCircleRight))
				{
					Log?.Error("Failed to find right border of the outer circle");
					return null;
				}
				if (!Tools.TryLineScan<int>(smallImage, new Rectangle(Point.Empty, smallImage.Size),
						LineScanDirection.TopToBottom,
						(count, color) => count + (Tools.ColorDiff(outerCircleColor, color) < colorThreshold ? 1 : 0),
						(count) => count > smallImage.Width / 20,
						out outerCircleTop))
				{
					Log?.Error("Failed to find top border of the outer circle");
					return null;
				}
				if (!Tools.TryLineScan<int>(smallImage, new Rectangle(Point.Empty, smallImage.Size),
						LineScanDirection.BottomToTop,
						(count, color) => count + (Tools.ColorDiff(outerCircleColor, color) < colorThreshold ? 1 : 0),
						(count) => count > smallImage.Width / 20,
						out outerCircleBottom))
				{
					Log?.Error("Failed to find bottom border of the outer circle");
					return null;
				}
				Log?.Info("Outer circle bounds: ({0}, {1}) - ({2}, {3})", outerCircleLeft, outerCircleTop, outerCircleRight, outerCircleBottom);
				var center = new Point((outerCircleLeft + outerCircleRight) / 2 * multiplier + multiplier / 2, (outerCircleTop + outerCircleBottom) / 2 * multiplier + multiplier / 2);
				var radius = (outerCircleRight - outerCircleLeft + outerCircleBottom - outerCircleTop) / 4.0 * multiplier + multiplier / 2;
				Log?.Info("Draftly taking {0} as a center and {1} as radius", center, radius);

				colorThreshold = (int)Tools.ColorDiff(backColor, outerCircleColor) / 2;
				Log?.Debug("Adjusting color threshold to {0} - half difference between back and outer circle color", colorThreshold);

				AdjustCenterAndRadius(image, outerCircleColor, colorThreshold, ref center, ref radius);
				Log?.Info("Adjusted center {0} and radius {1}", center, radius);

				ColorDouble innerCircleColor;
				Tools.TryGetAverageColor(image, new Rectangle(center.X - 5, center.Y - 5, 10, 10), 3 * 255, out innerCircleColor);
				Log?.Info("Taking color {0} from center of outer circle as color of inner circle", innerCircleColor);

				colorThreshold = (int)Math.Max(10, Tools.ColorDiff(outerCircleColor, innerCircleColor) / 2);
				Log?.Info("Taking {0} as color threshold for border of inner circle", colorThreshold);

				int innerCircleLeft = center.X - (int)radius, 
					innerCircleRight = center.X + (int)radius, 
					innerCircleTop = center.Y - (int)radius,
					innerCircleBottom = center.Y + (int)radius;
				for (var x = center.X; x > center.X - radius; x--)
					if (Tools.ColorDiff(innerCircleColor, image.GetPixel(x, center.Y)) >= colorThreshold)
					{
						innerCircleLeft = x;
						break;
					}
				for (var x = center.X; x < center.X + radius; x++)
					if (Tools.ColorDiff(innerCircleColor, image.GetPixel(x, center.Y)) >= colorThreshold)
					{
						innerCircleRight = x;
						break;
					}
				for (var y = center.Y; y > center.Y - radius; y--)
					if (Tools.ColorDiff(innerCircleColor, image.GetPixel(center.X, y)) >= colorThreshold)
					{
						innerCircleTop = y;
						break;
					}
				for (var y = center.Y; y < center.Y + radius; y++)
					if (Tools.ColorDiff(innerCircleColor, image.GetPixel(center.X, y)) >= colorThreshold)
					{
						innerCircleBottom = y;
						break;
					}
				Log?.Info("Inner circle bounds: ({0}, {1}) - ({2}, {3})", innerCircleLeft, innerCircleTop, innerCircleRight, innerCircleBottom);
				var innerCenter = new Point((innerCircleLeft + innerCircleRight) / 2, (innerCircleTop + innerCircleBottom) / 2);
				var innerRadius = (innerCircleRight - innerCircleLeft + innerCircleBottom - innerCircleTop) / 4.0;
				Log?.Info("Draftly taking {0} as a inner circle center and {1} as radius", innerCenter, innerRadius);

				AdjustCenterAndRadius(image, innerCircleColor, colorThreshold, ref innerCenter, ref innerRadius);
				Log?.Info("Adjusted inner circle center {0} and radius {1}", innerCenter, innerRadius);

				return new RecognizerResult {
					OuterCircleCenter = center,
					OuterCircleRadius = radius,
					InnerCircleCenter = innerCenter,
					InnerCircleRadius = innerRadius
				};
			}
			finally
			{
				if (needDispose)
					smallImage.Dispose();
			}
		}

		private void AdjustCenterAndRadius(Bitmap image, Color circleColor, int colorThreshold, ref Point center, ref double radius)
		{
			var percentage = FindOptimalRadius(image, circleColor, center, ref radius, colorThreshold);
			var initialMeasure = new Tuple<int, int, double, double>(center.X, center.Y, radius, percentage);
			var measures = new List<Tuple<int, int, double, double>> { initialMeasure };

			var maxPercentage = initialMeasure.Item4;
			Tuple<int, int, double, double> bestMeasure = initialMeasure;
			do
			{
				center = new Point(bestMeasure.Item1, bestMeasure.Item2);
				radius = bestMeasure.Item3;
				Log?.Debug("Selecting {0} (R={1}) as a hypothesis: {2:P2}", center, radius, maxPercentage);
				for (var dx = -1; dx <= 1; dx++)
					for (var dy = -1; dy <= 1; dy++)
					{
						var newCenter = new Point(center.X + dx, center.Y + dy);
						var measure = measures.Find((x) => x.Item1 == newCenter.X + dx && x.Item2 == newCenter.Y + dy);
						if (measure == null)
						{
							var aRadius = radius;
							percentage = FindOptimalRadius(image, circleColor, newCenter, ref aRadius, colorThreshold);
							measure = new Tuple<int, int, double, double>(center.X + dx, center.Y + dy, aRadius, percentage);
							Log?.Debug("Measure at {0}:{1} => R = {2}, W= {3:P2}", center.X + dx, center.Y + dy, aRadius, percentage);
							measures.Add(measure);
						}
						if (maxPercentage < measure.Item4)
						{
							maxPercentage = measure.Item4;
							bestMeasure = measure;
						}
					}
			}
			while (bestMeasure.Item1 != center.X || bestMeasure.Item2 != center.Y);
		}

		private double FindOptimalRadius(Bitmap image, Color circleColor, Point center, ref double radius, int colorThreshold)
		{
			var weightFunc = new Func<double, double, double>(
				(x, y) => x / 2 + (1 - y) / 2);
			var maxPercentage = 0d;
			var adjRadius = radius;
			var percentCircle = Tools.GetColorPercentOnCircle(image, circleColor, new Point(center.X, center.Y), radius - 2, colorThreshold);
			for (var dr = -5; dr < 10; dr++)
			{
				if (radius + dr < 2)
					continue;
				var nextPercentCircle = Tools.GetColorPercentOnCircle(image, circleColor, new Point(center.X, center.Y), radius + dr + 1, colorThreshold);
				if (nextPercentCircle < 0.1)
					break;
				var weight = weightFunc(percentCircle, nextPercentCircle);
				if (weight > maxPercentage)
				{
					maxPercentage = weight;
					adjRadius = radius + dr;
				}
				percentCircle = nextPercentCircle;
			}
			radius = adjRadius;
			return maxPercentage;
		}
	}
}
