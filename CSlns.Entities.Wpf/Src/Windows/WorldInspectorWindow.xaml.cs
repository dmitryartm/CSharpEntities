using System;
using System.Windows;


namespace CSlns.Entities.Wpf.Windows {
    public partial class WorldInspectorWindow : Window {
        public WorldInspectorWindow(World world) {
            this.InitializeComponent();

            this._ArchetypeListView.InitViewModel(world.Entities);
            this._ComponentSystemTreeView.InitViewModel(world.RootSystem);

            var subscription = world.RootSystem.AfterExecute.Subscribe(_ => {
                this._ArchetypeListView.ViewModel.Update();
                this._ComponentSystemTreeView.ViewModel.Update();
            });

            this.Closing += (sender, args) => subscription.Dispose();
        }
    }
}