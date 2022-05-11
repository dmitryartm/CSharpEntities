using System.Reactive.Disposables;
using CSlns.Entities.Systems;
using CSlns.Entities.Wpf.TreeListView.ViewModels;
using CSlns.Entities.Wpf.ViewModels;
using ReactiveUI;


namespace CSlns.Entities.Wpf.Views {


public partial class ComponentSystemTreeView : ReactiveUserControl<ComponentSystemTreeViewModel> {
    public ComponentSystemTreeView() {
        this.InitializeComponent();

        this.WhenActivated(disposable => {
            disposable.Add(Disposable.Create(() => this.ViewModel?.Dispose()));
        });
    }


    public void InitViewModel(IComponentSystem system) {
        var treeViewModel = TreeViewModel.Create<ComponentSystemTreeViewItemContent>();
        this.TreeView.ViewModel = treeViewModel;
        this.ViewModel = new ComponentSystemTreeViewModel(treeViewModel, system);
    }


    public void InitViewModel(World world) {
        this.InitViewModel(world.RootSystem);
    }

}


}