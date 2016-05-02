using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Slideshow.View.Converters
{
	public class IDebugVisibilityConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
#if DEBUG
			return Visibility.Visible;
#else
			return Visibility.Collapsed;
#endif
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
