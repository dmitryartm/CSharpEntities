using ECS.WPF.ViewModels;
using ReactiveUI;

namespace ECS.WPF.Views {
    public partial class ArchetypeListView : ReactiveUserControl<ArchetypeListViewModel> {
        public ArchetypeListView() {
            this.InitializeComponent();

            this.WhenActivated(disposable => {
                if (this.ViewModel == null) {
                    return;
                }

                this.listView.ItemsSource = this.ViewModel.Items;
            });
        }

        public void InitViewModel(EntityManager entities) {
            this.ViewModel = new ArchetypeListViewModel(entities);
        }
    }
}