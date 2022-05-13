using CSlns.Entities;
using CSlns.Entities.Systems;
using glTFViewer.World.Components;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Systems;


public class GpuBufferManagerSystem : ComponentSystem<MainWorld> {

    public GpuBufferManagerSystem(MainWorld world) : base(world) {
        this._createMeshDataGpu = this.Entities.ForEach(
            (ref MeshDataComponent data, ref MeshDataGpuComponent dataGpu) => {
                if (dataGpu.BuffersAreEmpty) {
                    dataGpu.CreateBuffers(this.Device, data);
                }
            });

        // this._createInstancesGpu =
        //     this.Entities
        //         .ForEach((ref MeshInvRefList invRefs, ref MeshInstancesDataGpu instancesGpu) => {
        //             if (invRefs.Entities.Count == 0) {
        //                 return;
        //             }
        //
        //             if (instancesGpu.Outlined != null || instancesGpu.Visible != null) {
        //                 DisposableRef.Dispose(ref instancesGpu.Visible);
        //                 DisposableRef.Dispose(ref instancesGpu.Outlined);
        //             }
        //
        //             instancesGpu.Visible = new VertexBuffer(this.Device, invRefs.Entities.Count * sizeof(MeshInstance),
        //                 Usage.WriteOnly, VertexFormat.None, Pool.Default);
        //             instancesGpu.Outlined = new VertexBuffer(this.Device, invRefs.Entities.Count * sizeof(Matrix),
        //                 Usage.WriteOnly, VertexFormat.None, Pool.Default);
        //         });


        // this._disposeInstancesGpu =
        //     this.Entities.ForEach((ref MeshInstancesDataGpu instancesGpu) => instancesGpu.Dispose());


        this._disposeMeshDataGpu = this.Entities.ForEach((ref MeshDataGpuComponent dataGpu) => dataGpu.Dispose());
    }


    #region ComponentSystem


    protected override void OnStart() {
        this.DeviceManager = this.World.Get<DeviceManagerSystem>();
    }


    protected override void OnExecute() {
        if (this.DeviceManager?.Device == null) {
            return;
        }
        
        if (this.DeviceManager.NewDeviceCreated) {
            this._disposeMeshDataGpu.Execute();
            // this._createInstancesGpu.Execute();
        }
        
        this._createMeshDataGpu.Execute();
    }


    #endregion


    #region Private


    private readonly QueryAction? _createInstancesGpu;
    private readonly QueryAction _createMeshDataGpu;
    private readonly QueryAction? _disposeInstancesGpu;
    private readonly QueryAction _disposeMeshDataGpu;


    private DeviceManagerSystem? DeviceManager;
    private DeviceEx Device => this.DeviceManager!.Device!;


    #endregion


}