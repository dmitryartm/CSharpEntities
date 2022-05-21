using CSharpEntities.Systems;


namespace CSharpEntities.Wpf.Windows {
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