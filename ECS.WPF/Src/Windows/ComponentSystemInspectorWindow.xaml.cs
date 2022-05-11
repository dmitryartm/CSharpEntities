namespace ECS.WPF.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ComponentSystemInspectorWindow {
        public ComponentSystemInspectorWindow(ComponentSystem system) {
            InitializeComponent();

            this.componentSystemTreeView.InitViewModel(system);
        }
    }
}