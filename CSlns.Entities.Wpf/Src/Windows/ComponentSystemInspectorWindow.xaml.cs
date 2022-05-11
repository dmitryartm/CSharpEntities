using CSlns.Entities.Systems;


namespace CSlns.Entities.Wpf.Windows {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ComponentSystemInspectorWindow {
        public ComponentSystemInspectorWindow(ComponentSystem system) {
            this.InitializeComponent();

            this.componentSystemTreeView.InitViewModel(system);
        }
    }
}