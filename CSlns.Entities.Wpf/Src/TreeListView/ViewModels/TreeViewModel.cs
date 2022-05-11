using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;


namespace CSlns.Entities.Wpf.TreeListView.ViewModels {
    public interface ITreeViewModel {
        IEnumerable<ITreeViewItemModel> Items { get; }
        IEnumerable<ITreeViewItemModel> AllItems { get; }
        IEnumerable<ITreeViewItemModel> SelectedItemsValue { get; }
        void ExpandAll();
        void CollapseAll();
        void DeselectAll();
        void UpdateSelectedItems();
        void UpdateItems();
    }

    internal interface ITreeViewModelImpl : ITreeViewModel {
        void DeselectInvisible();
        IObservable<ITreeViewItemModel> OnFocusItem { get; }
    }
    
    public interface ITreeViewModel<T> : ITreeViewModel where T : ITreeItemContent<T> {
        new ReadOnlyObservableCollection<ITreeViewItemModel<T>> Items { get; }
        new ImmutableArray<ITreeViewItemModel<T>> AllItems { get; }
        
        IReadOnlyBehaviour<ImmutableHashSet<ITreeViewItemModel<T>>> SelectedItems { get; }
        
        ITreeViewItemModel<T> Root { get; set; }
        ITreeViewItemModel<T> CreateRoot();
        
        void FocusItem(ITreeViewItemModel<T> item);
        Option<Predicate<T>> Filter { get; set; }
    }
    
    
    public static class TreeViewModel {
        public static ITreeViewModel<T> Create<T>() where T : ITreeItemContent<T> => new _TreeViewModel<T>();
    }
    
        
    internal class _TreeViewModel<T> : ITreeViewModel<T>, ITreeViewModelImpl where T : ITreeItemContent<T> {
        #region Private
        
        public _TreeViewModel() {
            this.ItemsSource = new SourceList<ITreeViewItemModel<T>>();

            this.ItemsSource
                .Connect()
                .Bind(out this._items)
                .Subscribe();

            this._rootBhv
                .Subscribe(root => {
                    this.UpdateItems();
                });
        }

        private bool _suppressExpandedChanged;

        private readonly ReadOnlyObservableCollection<ITreeViewItemModel<T>> _items;
        
        private SourceList<ITreeViewItemModel<T>> ItemsSource { get; }

        private readonly BehaviorSubject<ITreeViewItemModel<T>> _rootBhv = new BehaviorSubject<ITreeViewItemModel<T>>(null);

        private readonly Subject<ITreeViewItemModel<T>> _focusItemSubject = new Subject<ITreeViewItemModel<T>>();

        private void SetFilter(Option<Predicate<T>> filterOpt) {
            if (filterOpt.TryGetValue(out var filter)) {
                this.EnableFilter(filter);
            }
            else {
                this.DisableFilter();
            }

            this.UpdateVisibleItems();
        }

        private void EnableFilter(Predicate<T> filter) {
            this._suppressExpandedChanged = true;

            var processedParents = new HashSet<ITreeViewItemModel<T>>();
            foreach (var item in this.AllItems) {
                item.SetHideExpander(true);
                if (filter(item.Content)) {
                    item.SetVisibility(true);
                    var parent = item.Parent;
                    while (parent != null && !processedParents.Contains(parent)) {
                        parent.SetVisibility(true);
                        parent.SetHideExpander(false);
                        parent.IsExpanded = true;
                        processedParents.Add(parent);
                        parent = parent.Parent;
                    }
                }
                else {
                    item.SetVisibility(false);
                }
            }

            this._suppressExpandedChanged = false;
        }

        private void DisableFilter() {
            foreach (var item in this.AllItems) {
                item.SetVisibility(true);
                item.SetHideExpander(false);
            }
        }

        private void UpdateVisibleItems() {
            if (this.Root == null) {
                return;
            }
            
            var items = this.Root.SubTree();
            for (var i = 0; i < items.Count; ++i) {
                items[i].SetIndex(Option.Some(i));
            }

            this.ItemsSource.Edit(updater => {
                updater.Clear();
                updater.AddRange(items);
            });
        }

        internal void OnExpandedChanged(ITreeViewItemModel<T> item) {
            if (this._suppressExpandedChanged) {
                return;
            }
            
            if (item.Index.TryGetValue(out var itemIndex)) {
                if (item.IsExpanded) {
                    var tree = item.SubTree();
                    this.ItemsSource.InsertRange(tree, itemIndex + 1);

                    for (var i = itemIndex + 1; i < this.ItemsSource.Count; ++i) {
                        this.Items[i].SetIndex(Option.Some(i));
                    }
                }
                else {
                    var itemsToRemoveCount = 0;
                    for (var i = itemIndex + 1; i < this.Items.Count; ++i) {
                        this.Items[i].SetIndex(Option.None);
                        if (item.Level >= this.Items[i].Level) {
                            break;
                        }
                        ++itemsToRemoveCount;
                    }
                    this.ItemsSource.RemoveRange(itemIndex + 1, itemsToRemoveCount);

                    for (var i = itemIndex + 1; i < this.ItemsSource.Count; ++i) {
                        this.Items[i].SetIndex(Option.Some(i));
                    }
                }
            }
        }
        
        #endregion
        
        #region Public
        
        public ReadOnlyObservableCollection<ITreeViewItemModel<T>> Items => this._items;

        public ImmutableArray<ITreeViewItemModel<T>> AllItems { get; set; }

        IEnumerable<ITreeViewItemModel> ITreeViewModel.SelectedItemsValue => this.SelectedItems.Value;

        public ITreeViewItemModel<T> Root {
            get => this._rootBhv.Value;
            set {
                if (this.Root != value) {
                    this._rootBhv.OnNext(value);
                    this.UpdateVisibleItems();
                }
            }
        }

        public ITreeViewItemModel<T> CreateRoot() {
            return new TreeViewItemModel<T>(this);
        }

        public void FocusItem(ITreeViewItemModel<T> item) {
            if (!item.HasIndex()) {
                var parent = item.Parent;

                this._suppressExpandedChanged = true;
                while (parent != null && !parent.HasIndex()) {
                    parent.Expand();
                    parent = parent.Parent;
                }

                this._suppressExpandedChanged = false;
                parent?.Expand();
            }

            this._focusItemSubject.OnNext(item);
        }

        private readonly BehaviorSubject<Option<Predicate<T>>> _filterBhv = new BehaviorSubject<Option<Predicate<T>>>(default);

        public Option<Predicate<T>> Filter {
            get => this._filterBhv.Value;
            set {
                if (this.Filter.IsSome || value.IsSome) {
                    this._filterBhv.OnNext(value);
                }
            }
        }

        private readonly IBehaviour<ImmutableHashSet<ITreeViewItemModel<T>>> selectedItems = Behaviour.Create(ImmutableHashSet<ITreeViewItemModel<T>>.Empty);

        public IReadOnlyBehaviour<ImmutableHashSet<ITreeViewItemModel<T>>> SelectedItems => this.selectedItems;

        public void UpdateSelectedItems() {
            var newSelectedItems = this.SelectedItems.Value;
            foreach (var item in this.SelectedItems.Value) {
                if (!item.IsSelected) {
                    newSelectedItems = newSelectedItems.Remove(item);
                }
            }

            foreach (var item in this.AllItems) {
                if (item.IsSelected) {
                    newSelectedItems = newSelectedItems.Add(item);
                }
            }

            this.selectedItems.Value = newSelectedItems;
        }
        
        public void DeselectInvisible() {
            foreach (var item in this.SelectedItems.Value) {
                if (!item.HasIndex()) {
                     item.IsSelected = false;
                }
            }
        }

        public void ExpandAll() {
            if (this.Root == null) {
                return;
            }
            
            this._suppressExpandedChanged = true;
            foreach (var item in this.AllItems) {
                item.IsExpanded = true;
            }
            
            this._suppressExpandedChanged = false;
            this.UpdateVisibleItems();
        }

        public void CollapseAll() {
            if (this.Root == null) {
                return;
            }

            var buffer = new ITreeViewItemModel<T>[this.Items.Count];
            var i = 0;
            foreach (var item in this.Items) {
                buffer[i++] = item;
            }
            
            foreach (var item in buffer) {
                var hidden = item.Level > 1;
                this._suppressExpandedChanged = hidden;
                item.IsExpanded = false;
                if (hidden) {
                    item.SetIndex(default);
                }
            }

            this._suppressExpandedChanged = false;
        }

        public void DeselectAll() {
            foreach (var item in this.SelectedItems.Value) {
                item.IsSelected = false;
            }
        }

        IEnumerable<ITreeViewItemModel> ITreeViewModel.Items => this.Items;
        
        IEnumerable<ITreeViewItemModel> ITreeViewModel.AllItems => this.AllItems;

        public IObservable<ITreeViewItemModel> OnFocusItem => this._focusItemSubject;


        public void UpdateItems() {
            if (this.Root != null) {
                this.AllItems = this.Root.SubTree(true).ToImmutableArray();

                var sub =
                    this._filterBhv
                        .Subscribe(this.SetFilter);

                this._rootBhv
                    .Skip(1)
                    .Take(1)
                    .Subscribe(_ => sub.Dispose());
            }
            else {
                this.AllItems = ImmutableArray<ITreeViewItemModel<T>>.Empty;
                this.selectedItems.Value = ImmutableHashSet<ITreeViewItemModel<T>>.Empty;
                this.ItemsSource.Clear();
            }
        }

        #endregion
    }
}