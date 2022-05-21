using System;
using System.Reactive.Disposables;
using CSharpEntities.Wpf.ViewModels;
using ReactiveUI;


namespace CSharpEntities.Wpf.Views {
    public partial class ArchetypeListView : ReactiveUserControl<ArchetypeListViewModel> {
        public ArchetypeListView() {
            this.InitializeComponent();

            this.WhenActivated(disposable => {
                if (this.ViewModel == null) {
                    return;
                }

                this.listView.ItemsSource = this.ViewModel.Items;

                disposable.Add(Disposable.Create(() => {
                    this._subscription?.Dispose();
                    this._subscription = null;
                }));
            });
        }

        public void InitViewModel(EntityManager entities) {
            this.ViewModel = new ArchetypeListViewModel(entities);
        }


        public void InitViewModel(World world) {
            this.InitViewModel(world.Entities);
            this._subscription = world.RootSystem.AfterExecute.Subscribe(_ => this.ViewModel?.Update());
        }


        private IDisposable _subscription;

    }
}