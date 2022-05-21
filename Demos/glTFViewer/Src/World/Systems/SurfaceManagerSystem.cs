using CSharpEntities.Systems;
using glTFViewer.Utils;
using SharpDX;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Systems;


/// <summary>
/// Creates and manages rendering target surfaces, executes children only when surfaces are valid
/// </summary>
public class SurfaceManagerSystem : ComponentSystem<MainWorld> {

    public SurfaceManagerSystem(MainWorld world) : base(world) {
        this.ExecuteSubsystems = false;
    }


    #region ComponentSystem


    protected override void OnStart() {
        this.DeviceManager = this.World.Get<DeviceManagerSystem>();
    }


    protected override void OnExecute() {
        if (this.DeviceManager == null || this.Device == null) {
            return;
        }

        var createNewSurfaces =
            this.DeviceManager.NewDeviceCreated
            || this.SurfaceSizeChanged
            || this._renderSurface == null
            || this._imageSurface == null;

        var disposeOldSurfaces = this.DeviceManager.OldDeviceDisposed || createNewSurfaces;

        if (disposeOldSurfaces) {
            DisposableRef.Dispose(ref this._renderSurface);
            DisposableRef.Dispose(ref this._imageSurface);
        }


        if (createNewSurfaces && this.SurfaceSizeIsValid) {
            this._imageSurface = Surface.CreateRenderTarget(this.Device, this.SurfaceSize.Width,
                this.SurfaceSize.Height, Format.A8R8G8B8, MultisampleType.None, 0, true);
            this._renderSurface = Surface.CreateRenderTarget(this.Device, this.SurfaceSize.Width,
                this.SurfaceSize.Height, Format.A8R8G8B8, this._multisampleType, 0, false);

            var depthSurface = Surface.CreateDepthStencil(this.Device, this.SurfaceSize.Width,
                this.SurfaceSize.Height, Format.D16, this._multisampleType, 0, true);
            this.Device.DepthStencilSurface = depthSurface;
            depthSurface.Dispose();
        }

        this.ExecuteSubsystems =
            this._renderSurface != null && this._imageSurface != null && this.World.D3DImage.IsFrontBufferAvailable;
        this.SurfaceSizeChanged = false;
    }


    #endregion


    #region Public


    public Surface? RenderSurface => this._renderSurface;
    public Surface? ImageSurface => this._imageSurface;


    public Size2 SurfaceSize {
        get => this._surfaceSize;
        set {
            if (this._surfaceSize != value) {
                this._surfaceSize = value;
                this.SurfaceSizeChanged = true;
            }
        }
    }


    public bool SurfaceSizeChanged { get; private set; }


    public bool SurfaceSizeIsValid => this.SurfaceSize.Height > 0 && this.SurfaceSize.Width > 0;


    #endregion


    #region Private


    private Size2 _surfaceSize;
    private Surface? _renderSurface;
    private Surface? _imageSurface;
    private readonly MultisampleType _multisampleType = MultisampleType.FourSamples;


    private DeviceManagerSystem? DeviceManager;
    private DeviceEx? Device => this.DeviceManager?.Device;


    #endregion


}