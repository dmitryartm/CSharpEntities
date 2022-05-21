using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CSharpEntities.Wpf.TreeListView.ViewModels;


namespace CSharpEntities.Wpf.TreeListView.Views {
	public class TreeListViewItem : ListViewItem, INotifyPropertyChanged {
		private IDisposable _isSelectedSubs;
		
		private ITreeViewItemModel _model;

		public ITreeViewItemModel Model {
			get => this._model;
			set {
				if (this._model != value) {
					this._model = value;
					this._isSelectedSubs?.Dispose();
					this._isSelectedSubs = this._model?.IsSelectedChanged.Subscribe(selected => this.IsSelected = selected);

					this.DataContext = value?.Content;
				}
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			if (this.Model != null && this.Model.IsSelected && KeyboardEx.CtrlAndShiftUntouched) {
				var treeViewModel = (ITreeViewModelImpl)this.Model.TreeViewModel;
				treeViewModel.DeselectInvisible();
				treeViewModel.UpdateSelectedItems();
			}
			
			base.OnMouseLeftButtonDown(e);
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			switch (e.Key) {
				case Key.Right:
					e.Handled = true;
					if (!this.Model.IsExpanded) {
						this.Model.IsExpanded = true;
					}
					else if (this.Model.Children.Count > 0) {
						this.Model.Children.First().Focus();
					}

					break;
		
				case Key.Left:
					e.Handled = true;
					if (this.Model.IsExpanded && this.Model.IsExpandable) {
						this.Model.IsExpanded = false;
					}
					else if (this.Model.Level > 1){
						this.Model.Parent.Focus();
					}

					break;
		
				case Key.Subtract:
					e.Handled = true;
					this.Model.IsExpanded = false;
					break;
		
				case Key.Add:
					e.Handled = true;
					this.Model.IsExpanded = true;
					break;
				
				default:
					base.OnKeyDown(e);
					break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string name) {
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}