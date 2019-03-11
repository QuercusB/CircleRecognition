using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ImageRecognition.Utility
{
	/// <summary>
	/// Логика взаимодействия для LogBox.xaml
	/// </summary>
	public partial class LogBox : UserControl
	{
		public static DependencyProperty LogProperty =
			DependencyProperty.Register("Log", typeof(MemoryLog), typeof(LogBox),
				new FrameworkPropertyMetadata { DefaultValue = null, PropertyChangedCallback = LogChanged });

		public LogBox()
		{
			InitializeComponent();
		}


		public MemoryLog Log
		{
			get { return GetValue(LogProperty) as MemoryLog; }
			set { SetValue(LogProperty, value); }
		}

		private static void LogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is LogBox)
				((LogBox)d).LogChanged(e);
		}

		private void LogChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is MemoryLog)
				((MemoryLog)e.OldValue).MessageAdded -= LogMessageAdded;
			textBox.Text = "";
			if (e.NewValue is MemoryLog)
			{
				textBox.Text = ((MemoryLog)e.NewValue).
					Aggregate("", (s, message) => s + message.SimpleFormatted() + Environment.NewLine);
				textBox.CaretIndex = textBox.Text.Length;
				textBox.ScrollToEnd();
				((MemoryLog)e.NewValue).MessageAdded += LogMessageAdded;
			}
		}

		private void LogMessageAdded(object sender, MemoryLogMessageEventArgs args)
		{
			this.Dispatcher.BeginInvoke((Action)(() =>
			{
				textBox.AppendText(args.Message.SimpleFormatted() + Environment.NewLine);
				textBox.CaretIndex = textBox.Text.Length;
				textBox.ScrollToEnd();
			}));
		}
	}
}
