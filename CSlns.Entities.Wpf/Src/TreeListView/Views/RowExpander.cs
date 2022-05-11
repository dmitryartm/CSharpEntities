using System.Windows;
using System.Windows.Controls;


namespace CSlns.Entities.Wpf.TreeListView.Views {
	public class RowExpander : Control {
		static RowExpander() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
		}
	}
}
