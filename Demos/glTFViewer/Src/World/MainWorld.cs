using System.Threading;
using System.Windows.Interop;
using CSlns.Entities;
using CSlns.Entities.Systems;
using glTFViewer.World.Systems;
using Serilog;


namespace glTFViewer.World;


public class MainWorld : CSlns.Entities.World {

    public MainWorld(D3DImage image) {
        this.D3DImage = image;
        this.RootSystem = new InlineSystem(this, "MainWorld") {
            new GltfLoaderSystem(this),
            new DeviceManagerSystem(this) {
                new GpuBufferManagerSystem(this),
                new SurfaceManagerSystem(this) {
                    new CameraSystem(this),
                    new RenderSystem(this)
                }
            }
        };

        this.StartSystems();
    }


    public ILogger Logger => Log.Logger;
    
    
    public float DeltaTime { get; private set; }
    public D3DImage D3DImage { get; }


    public void Update(float deltaTime) {
        this.DeltaTime = deltaTime;
        this.ExecuteSystems();
    }

}