using System;
using System.Windows;

namespace ImageRecognition
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			//Uri uri = new Uri("PresentationFramework.Aero; V4.0.0.0; 31bf3856ad364e35; component\\themes / aero.normalcolor.xaml", UriKind.Relative);
			//Resources.MergedDictionaries.Add(LoadComponent(uri) as ResourceDictionary);
		}
	}
}
