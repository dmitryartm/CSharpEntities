using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CSlns.Entities.Wpf.TreeListView.ViewModels;


namespace CSlns.Entities.Wpf.TreeListView.Views {
	public class TreeListView : ListView {
		public TreeListView() {
			this.ItemContainerGenerator.StatusChanged += this.ItemContainerGeneratorStatusChanged;
		}

		private ITreeViewModelImpl _viewModel;

		private IDisposable _viewModelSub;

		public ITreeViewModel ViewModel {
			get => this._viewModel;
			set {
				this._viewModelSub?.Dispose();
				this._viewModelSub = null;
				
				this._viewModel = (ITreeViewModelImpl) value;
				if (this._viewModel != null) {
					this.ItemsSource = this._viewModel.Items;
					this._viewModelSub = new CompositeDisposable {
						this._viewModel.OnFocusItem.Subscribe(this.FocusItem),
					};
				}
			}
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
			var changed = false;
			
			foreach (ITreeViewItemModel item in e.AddedItems) {
				if (item.HasIndex() && !item.IsSelected) {
					item.IsSelected = true;
					changed = true;
				}
			}

			foreach (ITreeViewItemModel itemContent in e.RemovedItems) {
				var item = itemContent;
				if (item.HasIndex() && item.IsSelected) {
					item.IsSelected = false;
					changed = true;
				}
			}
			
			if (changed && KeyboardEx.CtrlAndShiftUntouched) {
				this._viewModel.DeselectInvisible();
			}

			this._viewModel.UpdateSelectedItems();
			
			base.OnSelectionChanged(e);
		}

		protected override DependencyObject GetContainerForItemOverride() {
			return new TreeListViewItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item) {
			return item is TreeListViewItem;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject elementObj, object itemObj) {
			if (elementObj is TreeListViewItem element && itemObj is ITreeViewItemModel item) {
				element.Model = item;
				base.PrepareContainerForItemOverride(element, item.Content);
			}
		}

		protected override void ClearContainerForItemOverride(DependencyObject elementObj, object itemObj) {
			if (elementObj is TreeListViewItem element && itemObj is ITreeViewItemModel item) {
				element.Model = null;
				base.ClearContainerForItemOverride(element, item.Content);
			}
		}

		private ITreeViewItemModel PendingFocusItem;

        internal void FocusItem(ITreeViewItemModel item) {
            if (item.HasIndex()) {
	            this.ScrollIntoView(item);
	            if (this.ItemContainerGenerator.ContainerFromItem(item) is TreeListViewItem itemView) {
                    itemView.Focus();
                }
                else {
	                this.PendingFocusItem = item;
                }
            }
        }
        
		private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e) {
			if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && this.PendingFocusItem != null) {
				if (this.ItemContainerGenerator.ContainerFromItem(this.PendingFocusItem) is TreeListViewItem item) {
					item.Focus();
					this.PendingFocusItem = null;
				}
			}
		}
	}
}