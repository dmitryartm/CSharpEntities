using System;
using System.IO;
using System.Reflection;
using System.Windows;
using glTFViewer.World;
using glTFViewer.World.Systems;
using Microsoft.Win32;
using Serilog;


namespace glTFViewer.Views;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    public MainWindow() {
        this.InitializeComponent();

        this._world = new MainWorld(this._DXControl.D3DImage);
        this._gltfLoader = this._world.Get<GltfLoaderSystem>();
            
        this._SystemTree.InitViewModel(this._world);
        this._ArchetypeList.InitViewModel(this._world);

        this._DXControl.DXRender.Subscribe(render => {
            this._world.Update(render.DeltaTime);
        });

        var surfaceManagerSystem = this._world.Get<SurfaceManagerSystem>();
        this._DXControl.ImageSizeObs.Subscribe(size => {
            surfaceManagerSystem.SurfaceSize = size;
        });

        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var simpleSceneFile = Path.Combine(dir, @"Resources\Models\simple_scene.gltf");
        this._gltfLoader.LoadFile(simpleSceneFile);
    }


    private readonly MainWorld _world;
    private readonly GltfLoaderSystem _gltfLoader;


    private void _OpenFileMenuItem_OnClick(object sender, RoutedEventArgs e) {
        var dialog = new OpenFileDialog {
            Filter = "glTF|*gltf;*.glb",
        };
        
        if (dialog.ShowDialog(this) == true) {
            var fileName = dialog.FileName;
            this._gltfLoader.LoadFile(fileName);
        }
    }


    private void _ClearMenuItem_OnClick(object sender, RoutedEventArgs e) {
        this._gltfLoader.Unload();
    }
    
}