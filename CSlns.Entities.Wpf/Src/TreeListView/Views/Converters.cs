using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace CSlns.Entities.Wpf.TreeListView.Views {
	/// <summary>
	/// Convert Level to left margin
	/// </summary>
	internal class LevelToIndentConverter : IValueConverter {
		private const double IndentSize = 20.0;

		public object Convert(object obj, Type type, object parameter, CultureInfo culture) {
			if (obj is int level) {
				return new Thickness(level * IndentSize, 0, 0, 0);
			}
			else {
				throw new NotSupportedException();
			}
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}

	internal class CanExpandConverter : IValueConverter {
		public object Convert(object obj, Type type, object parameter, CultureInfo culture) {
			if (obj is bool visible) {
				return visible ? Visibility.Visible : Visibility.Hidden;
			}
			else {
				throw new NotSupportedException();
			}
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
