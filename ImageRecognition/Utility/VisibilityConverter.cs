using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageRecognition.Utility
{
	public class VisibilityConverter : IValueConverter
	{
		public static VisibilityConverter CollapsedIfFalse =
			new VisibilityConverter(false, Visibility.Collapsed, Visibility.Visible);

		public static VisibilityConverter CollapsedIfTrue =
			new VisibilityConverter(true, Visibility.Collapsed, Visibility.Visible);

		public static VisibilityConverter CollapsedIfNull =
			new VisibilityConverter(null, Visibility.Collapsed, Visibility.Visible);

		private object testValue;
		private Visibility trueValue, falseValue;

		public VisibilityConverter(object testValue, Visibility trueValue, Visibility falseValue)
		{
			this.testValue = testValue;
			this.trueValue = trueValue;
			this.falseValue = falseValue;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Equals(value, testValue) ? trueValue : falseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
