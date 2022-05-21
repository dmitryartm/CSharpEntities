using System;
using System.Reactive;
using System.Reactive.Subjects;
using CSharpEntities.Systems;
using glTFViewer.Utils;
using Serilog;
using SharpDX;
using SharpDX.Direct3D9;
using DXDeviceState = SharpDX.Direct3D9.DeviceState;


namespace glTFViewer.World.Systems;


/// <summary>
/// Creates and manages Direct3D device objects, executes children only when device is available
/// </summary>
public class DeviceManagerSystem : ComponentSystem<MainWorld> {

    public DeviceManagerSystem(MainWorld world) : base(world) {
        this._dummyHwnd = Direct3DLib.Inst.CreateDummyWindow();
        this._d3d = new Direct3DEx();

        this._presentParameters = new PresentParameters {
            Windowed = true,
            BackBufferHeight = 1,
            BackBufferWidth = 1,
            SwapEffect = SwapEffect.Discard,
            MultiSampleType = MultisampleType.FourSamples,
            MultiSampleQuality = 0,
        };

        this.OnDispose.Subscribe(_ => {
            this.DispatchDisposeDeviceRes();

            this._createDeviceResSubject.OnCompleted();
            this._disposeDeviceResSubject.OnCompleted();

            this.Device?.Dispose();
            this.Device = null;

            this._d3d?.Dispose();
            this._d3d = null;
        });
    }


    #region ComponentSystem


    protected override void OnExecute() {
        try {
            this.ExecuteSubsystems = false;

            var disposeDevice = false;
            var resetDevice = false;
            var createDeviceRes = false;


            #region Check Device State


            if (this.Device != null) {
                this.DeviceState = this.Device.CheckDeviceState(IntPtr.Zero);

                switch (this.DeviceState) {
                    case DXDeviceState.Ok:
                    case DXDeviceState.DeviceLost:
                    case DXDeviceState.PresentOccluded:
                        break;
                    case DXDeviceState.DeviceHung:
                        resetDevice = true;
                        break;
                    default:
                        disposeDevice = true;
                        break;
                }

                if (this.DeviceState != DeviceState.Ok) {
                    this.Logger.Debug("Device State {State}", this.DeviceState);
                }
            }


            #endregion


            #region Dispose Device Resources


            if (disposeDevice || resetDevice) {
                this.Logger.Debug("Dispose Device Resources Begin");

                this.DispatchDisposeDeviceRes();

                this.Logger.Debug("Dispose Device Resources End");
            }


            #endregion


            #region Dispose Device


            if (disposeDevice) {
                this.Logger.Debug("Dispose Device");
                this.Device?.Dispose();
                this.Device = null;
            }


            #endregion


            #region Create Device


            if (this.Device == null) {
                this.Device = new DeviceEx(this._d3d, 0, DeviceType.Hardware, this._dummyHwnd,
                    CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                    this._presentParameters);
                this.InitDevice();

                createDeviceRes = true;
            }


            #endregion


            #region Reset Device


            if (resetDevice) {
                try {
                    this.Device.ResetEx(ref this._presentParameters);
                    this.DeviceState = DXDeviceState.Ok;
                    this.InitDevice();

                    createDeviceRes = true;
                }
                catch (SharpDXException ex) {
                    var deviceState = (DeviceState) ex.ResultCode.Code;
                    switch (deviceState) {
                        case DXDeviceState.DeviceLost:
                        case DXDeviceState.DeviceHung:
                        case DXDeviceState.DeviceRemoved:
                            this.DeviceState = deviceState;
                            break;
                        default:
                            throw new Exception($"Unexpected device state {deviceState}");
                    }
                }
            }


            #endregion


            #region Create Device Resources


            if (createDeviceRes) {
                this.DispatchCreateDeviceRes();
            }


            #endregion


            var canRender = this.DeviceState == DeviceState.Ok;
            if (canRender) {
                this.ExecuteSubsystems = true;
            }
        }
        catch (Exception ex) {
            throw new Exception($"{nameof(DeviceManagerSystem)}.{nameof(this.OnExecute)} error", ex);
        }
    }


    #endregion


    #region Public


    public DeviceEx? Device { get; private set; }


    public DeviceState DeviceState { get; private set; }

    public IObservable<Unit> DisposeDeviceRes => this._disposeDeviceResSubject;
    public IObservable<Unit> CreateDeviceRes => this._createDeviceResSubject;


    #endregion


    #region Private


    private ILogger Logger => this.World.Logger;

    
    private void DispatchDisposeDeviceRes() => this._disposeDeviceResSubject.OnNext(Unit.Default);
    private void DispatchCreateDeviceRes() => this._createDeviceResSubject.OnNext(Unit.Default);


    private void InitDevice() {
        if (this.Device == null) {
            throw new NullReferenceException("Device cannot be null");
        }
        
        this.Device.SetRenderState(RenderState.Lighting, true);
        this.Device.SetRenderState(RenderState.ZEnable, true);
        this.Device.SetRenderState(RenderState.CullMode, Cull.None);
        this.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        this.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        this.Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
        this.Device.SetRenderState(RenderState.MultisampleAntialias, true);

        var vertexElems = new[] {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
            new VertexElement(0, 24, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),

            new VertexElement(1, 0, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 1),
            new VertexElement(1, 16, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 2),
            new VertexElement(1, 32, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 3),
            new VertexElement(1, 48, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 4),
            new VertexElement(1, 64, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 1),

            VertexElement.VertexDeclarationEnd
        };

        using (var vertexDeclaration = new VertexDeclaration(this.Device, vertexElems)) {
            this.Device.VertexDeclaration = vertexDeclaration;
        }

        this.Device.SetStreamSourceFrequency(1, 1, StreamSource.InstanceData);
    }
    
    
    public bool OldDeviceDisposed { get; }
    
    
    public bool NewDeviceCreated { get; }


    #region Fields


    private Direct3DEx? _d3d;
    private IntPtr _dummyHwnd;
    private PresentParameters _presentParameters;

    private readonly Subject<Unit> _disposeDeviceResSubject = new Subject<Unit>();
    private readonly Subject<Unit> _createDeviceResSubject = new Subject<Unit>();


    #endregion


    #endregion


}