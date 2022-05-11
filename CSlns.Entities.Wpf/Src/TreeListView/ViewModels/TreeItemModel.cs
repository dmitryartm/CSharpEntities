using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;


namespace CSlns.Entities.Wpf.TreeListView.ViewModels {
    public interface ITreeItemContent {
        ITreeViewItemModel TreeItem { get; set; }
    }

    public interface ITreeItemContent<T> : ITreeItemContent {
        new ITreeViewItemModel<T> TreeItem { get; set; }
    }
    
    public interface ITreeViewItemModel {
        Option<int> Index { get; }
        int Level { get; }
        bool IsExpanded { get; set; }
        bool IsExpandable { get; }
        bool IsVisible { get; }
        bool IsSelected { get; set; }
        ITreeViewItemModel Parent { get; }
        IReadOnlyList<ITreeViewItemModel> Children { get; }
        void Focus();
        ITreeItemContent Content { get; }
        IObservable<bool> IsSelectedChanged { get; }
        ITreeViewModel TreeViewModel { get; }
    }

    public interface ITreeViewItemModel<T> : ITreeViewItemModel {
        new T Content { get; }
        ITreeViewItemModel<T> AddChild(T value);
        void RemoveChild(ITreeViewItemModel<T> item);
        void SortChildren(Func<T, T, int> comparer);
        new ITreeViewItemModel<T> Parent { get; }
        new IReadOnlyList<ITreeViewItemModel<T>> Children { get; }
        List<ITreeViewItemModel<T>> SubTree(bool includeHidden = false);
    }

    public static class TreeItemModel {
        public static void Expand(this ITreeViewItemModel item) => item.IsExpanded = true;
        public static void Collapse(this ITreeViewItemModel item) => item.IsExpanded = false;
        public static bool HasIndex(this ITreeViewItemModel item) => item.Index.IsSome;

        internal static void SetIndex<T>(this ITreeViewItemModel<T> item, Option<int> index) where T : ITreeItemContent<T> {
            var itemMut = (TreeViewItemModel<T>)item;
            itemMut.Index = index;
        }

        internal static void SetVisibility<T>(this ITreeViewItemModel<T> item, bool visible) where T : ITreeItemContent<T> =>
            ((TreeViewItemModel<T>) item).IsVisible = visible;

        internal static void SetHideExpander<T>(this ITreeViewItemModel<T> item, bool value) where T : ITreeItemContent<T> =>
            ((TreeViewItemModel<T>) item).HideExpander = value;
    }
    
    internal class TreeViewItemModel<T> : ReactiveObject, ITreeViewItemModel<T> where T : ITreeItemContent<T> {
        #region Private
        
        internal TreeViewItemModel(_TreeViewModel<T> treeViewModel) {
            this.Level = 0;
            this._isExpanded = true;
            this.TreeViewModel = treeViewModel;
        }
        
        internal TreeViewItemModel(TreeViewItemModel<T> parent, T value) {
            this.Level = parent.Level + 1;
            
            value.TreeItem = this;
            this.Content = value;

            this.Parent = parent;
            parent._children.Add(this);

            this.TreeViewModel = parent.TreeViewModel;
        }

        private bool _isExpanded;

        private bool _isSelected;
        
        private readonly List<TreeViewItemModel<T>> _children = new List<TreeViewItemModel<T>>();

        #endregion
        
        #region Public
        
        public bool HideExpander { get; set; }
        
        public _TreeViewModel<T> TreeViewModel { get; }

        ITreeViewModel ITreeViewItemModel.TreeViewModel => this.TreeViewModel;
        
        public Option<int> Index { get; set; }
        
        public int Level { get; }

        public bool IsExpanded {
            get => this._isExpanded;
            set {
                if (this._isExpanded != value) {
                    this._isExpanded = value;
                    this.RaisePropertyChanged(nameof(this.IsExpanded));
                    this.TreeViewModel.OnExpandedChanged(this);
                }
            }
        }

        public bool IsExpandable => !this.HideExpander && this.Children.Any();
        
        public bool IsVisible { get; set; } = true;

        public bool IsSelected {
            get => this._isSelected;
            set {
                if (this._isSelected != value) {
                    this._isSelected = value;
                    this.RaisePropertyChanged(nameof(this.IsSelected));
                }
            }
        }

        public T Content { get; }

        public IObservable<bool> IsSelectedChanged => this.WhenAnyValue(x => x.IsSelected);

        public ITreeViewItemModel<T> Parent { get; }

        public IReadOnlyList<ITreeViewItemModel<T>> Children => this._children;

        public List<ITreeViewItemModel<T>> SubTree(bool includeHidden = false) {
            return this.SubTree(new List<ITreeViewItemModel<T>>(), includeHidden);
        }

        public List<ITreeViewItemModel<T>> SubTree(List<ITreeViewItemModel<T>> buffer, bool includeHidden = true) {
            static List<ITreeViewItemModel<T>> subTree(ITreeViewItemModel<T> item, List<ITreeViewItemModel<T>> tree, bool includeHidden) {
                if (includeHidden || item.IsExpanded) {
                    foreach (var child in item.Children) {
                        if (child.IsVisible) {
                            tree.Add(child);
                            subTree(child, tree, includeHidden);
                        }
                    }
                }

                return tree;
            }

            buffer.Clear();
            return subTree(this, buffer, includeHidden);
        }

        public ITreeViewItemModel<T> AddChild(T value) {
            return new TreeViewItemModel<T>(this, value);
        }

        public void RemoveChild(ITreeViewItemModel<T> child) {
            this._children.Remove(child as TreeViewItemModel<T>);
        }

        public void SortChildren(Func<T, T, int> comparer) {
            this._children.Sort((a,  b) => comparer(a.Content, b.Content));
        }

        public void Focus() {
            this.TreeViewModel.FocusItem(this);
        }
        
        ITreeViewItemModel ITreeViewItemModel.Parent => this.Parent;

        IReadOnlyList<ITreeViewItemModel> ITreeViewItemModel.Children => this.Children;

        ITreeItemContent ITreeViewItemModel.Content => this.Content;

        #endregion
    }
}