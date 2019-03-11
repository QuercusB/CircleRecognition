using ImageRecognition.Utility;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;

namespace ImageRecognition.ImagePage
{
	public class ImagePageVM: INotifyPropertyChanged
	{
		private ImageSource image;
		private readonly MemoryLog log = new MemoryLog();
		private readonly Recognizer.Recognizer recognizer = new Recognizer.Recognizer();
		private bool isRecognizing = false;
		private bool isRecognized = false;
		private bool isRecognitionFailed = false;
		private Recognizer.RecognizerResult recognizerResult = null;

		public ImagePageVM()
		{
			Dispatcher = Dispatcher.CurrentDispatcher;
			Load = new RelayCommand(DoLoad);
			recognizer.Log = log;
		}

		public MemoryLog Log
		{
			get { return log; }
		}

		public ImageSource Image
		{
			get { return image; }
			set { SetProperty(ref image, value, "Image"); }
		}

		public bool IsRecognizing
		{
			get { return isRecognizing; }
			set { SetProperty(ref isRecognizing, value, "IsRecognizing"); }
		}

		public bool IsRecognized
		{
			get { return isRecognized; }
			set { SetProperty(ref isRecognized, value, "IsRecognized"); }
		}

		public bool IsRecognitionFailed
		{
			get { return isRecognitionFailed; }
			set { SetProperty(ref isRecognitionFailed, value, "IsRecognitionFailed"); }
		}

		public Recognizer.RecognizerResult RecognizerResult
		{
			get { return recognizerResult; }
			set { SetProperty(ref recognizerResult, value, "RecognizerResult"); }
		}

		public Dispatcher Dispatcher { get; private set; }

		public RelayCommand Load { get; private set; }

		public void DoLoad()
		{
			var dialog = new OpenFileDialog
			{
				Title = "Select image file",
				CheckFileExists = true,
				Filter = "Image files|*.jpg;*.png;*.bmp;*.tiff|All files|*.*",
				FilterIndex = 0,
				Multiselect = false
			};
			if (dialog.ShowDialog() != true)
				return;
			var image = (Bitmap)System.Drawing.Image.FromFile(dialog.FileName);
			Image = image.ToBitmapImage();
			IsRecognizing = true;
			IsRecognitionFailed = false;
			RecognizerResult = null;
			ThreadPool.QueueUserWorkItem((_) =>
			{
				try
				{
					var result = recognizer.Run(image);

					if (result != null)
					{
						RecognizerResult = result;
						using (var g = Graphics.FromImage(image))
						{
							g.DrawLine(Pens.White, 0, result.OuterCircleCenter.Y, image.Width - 1, result.OuterCircleCenter.Y);
							g.DrawLine(Pens.White, result.OuterCircleCenter.X, 0, result.OuterCircleCenter.X, image.Height - 1);

							g.DrawEllipse(Pens.White, (int)(result.OuterCircleCenter.X - result.OuterCircleRadius), (int)(result.OuterCircleCenter.Y - result.OuterCircleRadius), (int)(2 * result.OuterCircleRadius), (int)(2 * result.OuterCircleRadius));

							g.DrawEllipse(Pens.White, (int)(result.InnerCircleCenter.X - result.InnerCircleRadius), (int)(result.InnerCircleCenter.Y - result.InnerCircleRadius), (int)(2 * result.InnerCircleRadius), (int)(2 * result.InnerCircleRadius));
						}
					}
					else
						isRecognitionFailed = true;

					Dispatcher.InvokeAsync(() =>
					{
						Image = image.ToBitmapImage();
						image.Dispose();
						IsRecognizing = false;
					});
				}
				catch
				{
					image.Dispose();
				}
			});
			//	var image = new BitmapImage();
			//image.BeginInit();
			//image.UriSource = new Uri("samples/geom_anf1-22108-A.jpg", UriKind.Relative);
			//image.CacheOption = BitmapCacheOption.OnLoad;
			//image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			//image.EndInit();
			//Image = image;
		}

		public void DoRecognize()
		{
		}

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged = null;

		private void OnPropertyChange(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SetProperty<T>(ref T field, T value, string propertyName)
		{
			if (Equals(field, value))
				return;
			field = value;
			Dispatcher.InvokeAsync(() =>
				OnPropertyChange(propertyName));
		}

		#endregion
	}
}
