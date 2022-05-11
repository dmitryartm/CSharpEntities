using CSlns.Entities.Systems;
using CSlns.Entities.Wpf.TreeListView.ViewModels;
using CSlns.Entities.Wpf.ViewModels;
using ReactiveUI;


namespace CSlns.Entities.Wpf.Views {
    public partial class ComponentSystemTreeView : ReactiveUserControl<ComponentSystemTreeViewModel> {
        public ComponentSystemTreeView() {
            this.InitializeComponent();
        }

        public void InitViewModel(IComponentSystem system) {
            var treeViewModel = TreeViewModel.Create<ComponentSystemTreeViewItemContent>();
            this.TreeView.ViewModel = treeViewModel;
            this.ViewModel = new ComponentSystemTreeViewModel(treeViewModel, system);
        }
    }
}