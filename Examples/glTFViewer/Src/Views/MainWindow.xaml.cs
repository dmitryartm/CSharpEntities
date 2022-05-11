using System;
using System.Windows;
using glTFViewer.World;
using Serilog;


namespace glTFViewer.Views;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    public MainWindow() {
        this.InitializeComponent();

        this._world = new MainWorld();
        this._SystemTree.InitViewModel(this._world);
        this._ArchetypeList.InitViewModel(this._world);

        this._DXControl.DXRender.Subscribe(render => {
            try {
                this._world.ExecuteSystems();
            }
            catch (Exception ex) {
                const string msg = "OnRender error";
                Log.Error(ex, msg);
                MessageBox.Show(msg + Environment.NewLine + ex.Message, "ERROR", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        });
    }


    private readonly CSlns.Entities.World _world;

}