using CSlns.Wpf.TreeListView.ViewModels;
using ECS.WPF.ViewModels;
using ReactiveUI;

namespace ECS.WPF.Views {
    public partial class ComponentSystemTreeView : ReactiveUserControl<ComponentSystemTreeViewModel> {
        public ComponentSystemTreeView() {
            InitializeComponent();
        }

        public void InitViewModel(IComponentSystem system) {
            var treeViewModel = TreeViewModel.Create<ComponentSystemTreeViewItemContent>();
            this.TreeView.ViewModel = treeViewModel;
            this.ViewModel = new ComponentSystemTreeViewModel(treeViewModel, system);
        }
    }
}