using System;
using CSlns.Entities.Systems;
using CSlns.Entities.Wpf.TreeListView.ViewModels;
using ReactiveUI;


namespace CSlns.Entities.Wpf.ViewModels {


public class ComponentSystemTreeViewModel : ReactiveObject, IDisposable {
    public ComponentSystemTreeViewModel(ITreeViewModel<ComponentSystemTreeViewItemContent> componentSystemTree,
        IComponentSystem worldSystem) {
        this.ComponentSystemTree = componentSystemTree;
        this._worldSystem = worldSystem;

        this.Update();
    }


    public ITreeViewModel<ComponentSystemTreeViewItemContent> ComponentSystemTree { get; }


    public void Update() {
        if (this._version != this._worldSystem.Version) {
            var root = this.ComponentSystemTree.CreateRoot();
            CreateItems(root, this._worldSystem);
            this.ComponentSystemTree.Root = root;
            this.ComponentSystemTree.ExpandAll();


            void CreateItems(ITreeViewItemModel<ComponentSystemTreeViewItemContent> parentItem,
                IComponentSystem system) {
                var item = parentItem.AddChild(new ComponentSystemTreeViewItemContent(system));

                foreach (var childSystem in system.Subsystems) {
                    CreateItems(item, childSystem);
                }
            }


            this._version = this._worldSystem.Version;
        }
    }


    public void Dispose() {
        foreach (var item in this.ComponentSystemTree.AllItems) {
            item.Content.Dispose();
        }
    }


    private int _version = -1;
    private readonly IComponentSystem _worldSystem;

}


}