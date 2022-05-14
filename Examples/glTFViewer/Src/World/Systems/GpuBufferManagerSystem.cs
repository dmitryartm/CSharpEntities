using System;
using CSlns.Entities;
using CSlns.Entities.Systems;
using glTFViewer.Utils;
using glTFViewer.World.Components;
using SharpDX;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Systems;


public class GpuBufferManagerSystem : ComponentSystem<MainWorld> {

    public unsafe GpuBufferManagerSystem(MainWorld world) : base(world) {
        this._createMeshDataGpu = this.Entities.ForEach(
            (ref MeshDataComponent data, ref MeshDataGpuComponent dataGpu) => {
                if (dataGpu.BuffersAreEmpty) {
                    dataGpu.CreateBuffers(this.Device, data);
                }
            });

        this._createInstancesGpu =
            this.Entities
                .ForEach((ref MeshInstanceArray instances, ref MeshInstanceArrayGpu instancesGpu) => {
                    if (instancesGpu.BufferIsEmpty) {
                        var sizeofMatrix = sizeof(Matrix);
                        instancesGpu.Count = instances.TransformMatrices.Length;
                        instancesGpu.Stride = sizeofMatrix;
                        instancesGpu.TransformMatrices = new VertexBuffer(
                            this.Device, instances.TransformMatrices.Length * sizeofMatrix,
                            Usage.WriteOnly, VertexFormat.None, Pool.Default);
                        instancesGpu.TransformMatrices.Lock(0, 0, LockFlags.None)
                            .WriteRange(instances.TransformMatrices, 0, instances.TransformMatrices.Length);
                        instancesGpu.TransformMatrices.Unlock();
                    }
                });


        this._disposeInstancesGpu =
            this.Entities.ForEach((ref MeshInstanceArrayGpu instancesGpu) => instancesGpu.Dispose());


        this._disposeMeshDataGpu = this.Entities.ForEach((ref MeshDataGpuComponent dataGpu) => dataGpu.Dispose());
    }


    #region ComponentSystem


    protected override void OnStart() {
        this.DeviceManager = this.World.Get<DeviceManagerSystem>();

        this.DeviceManager.DisposeDeviceRes.Subscribe(_ => {
            this._disposeMeshDataGpu.Execute();
            this._disposeInstancesGpu.Execute();
        });
    }


    protected override void OnExecute() {
        if (this.DeviceManager?.Device == null) {
            return;
        }

        this._createMeshDataGpu.Execute();
        this._createInstancesGpu.Execute();
    }


    #endregion


    #region Private


    private readonly QueryAction _createInstancesGpu;
    private readonly QueryAction _createMeshDataGpu;
    private readonly QueryAction _disposeInstancesGpu;
    private readonly QueryAction _disposeMeshDataGpu;


    private DeviceManagerSystem? DeviceManager;
    private DeviceEx Device => this.DeviceManager!.Device!;


    #endregion


}