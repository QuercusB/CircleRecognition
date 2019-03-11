using ImageRecognition.ImagePage;
using System.Windows;

namespace ImageRecognition
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.DataContext = new ImagePageVM();
			InitializeComponent();
		}
	}
}
