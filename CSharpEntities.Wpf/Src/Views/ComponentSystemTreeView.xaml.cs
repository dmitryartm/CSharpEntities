using System.Reactive.Disposables;
using CSharpEntities.Systems;
using CSharpEntities.Wpf.TreeListView.ViewModels;
using CSharpEntities.Wpf.ViewModels;
using ReactiveUI;


namespace CSharpEntities.Wpf.Views {


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