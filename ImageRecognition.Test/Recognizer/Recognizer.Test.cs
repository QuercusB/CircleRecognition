using ImageRecognition.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageRecognition.Recognizer.Test
{
	[TestClass]
	//[Ignore("Explicit")]
	public class RecognizerTest
	{
		private string[] samples;
		private string outputFolder;
		private static readonly ILog log = new ConsoleLog();
		private Recognizer recognizer;

		public RecognizerTest()
		{
			var samplesFolder = Path.Combine(Environment.CurrentDirectory, "samples");
			outputFolder = Path.Combine(Environment.CurrentDirectory, "samples-out");
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);
			samples = Directory.GetFiles(samplesFolder, "*.jpg");
			recognizer = new Recognizer();
			recognizer.Log = log;
		}

		[TestMethod]
		public void Recognize_Samples()
		{
			foreach (var samplePath in samples)
			{
				//var samplePath = samples[0];
				var sampleName = Path.GetFileName(samplePath);
				var image = Image.FromFile(samplePath) as Bitmap;

				log.Info("Runing recognition for {0}", sampleName);
				var result = recognizer.Run(image);
				using (var g = Graphics.FromImage(image))
				{
					g.DrawLine(Pens.White, 0, result.OuterCircleCenter.Y, image.Width - 1, result.OuterCircleCenter.Y);
					g.DrawLine(Pens.White, result.OuterCircleCenter.X, 0, result.OuterCircleCenter.X, image.Height - 1);

					g.DrawEllipse(Pens.White, (int)(result.OuterCircleCenter.X - result.OuterCircleRadius), (int)(result.OuterCircleCenter.Y - result.OuterCircleRadius), (int)(2 * result.OuterCircleRadius), (int)(2 * result.OuterCircleRadius));
				}
				image.Save(Path.Combine(outputFolder, sampleName), ImageFormat.Jpeg);
				break;
			}
		}
	}
}
