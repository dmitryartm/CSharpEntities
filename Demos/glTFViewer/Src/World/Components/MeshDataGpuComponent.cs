using System;
using System.Reactive.Disposables;
using glTFViewer.Utils;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Components;


public struct MeshDataGpuComponent : IDisposable {
    public VertexBuffer? Vertices;
    public IndexBuffer? Indices;
    public int VerticesCount;
    public int TrianglesCount;


    public bool BuffersAreEmpty => this.Vertices == null || this.Indices == null;


    public unsafe void CreateBuffers(DeviceEx device, in MeshDataComponent mesh) {
        this.VerticesCount = mesh.Vertices.Length;
        this.TrianglesCount = mesh.Indices.Length / 3;

        this.Vertices = new VertexBuffer(device, mesh.Vertices.Length * sizeof(VertexData),
            Usage.WriteOnly, VertexFormat.None, Pool.Default);
        this.Vertices.Lock(0, 0, LockFlags.None).WriteRange(mesh.Vertices);
        this.Vertices.Unlock();

        this.Indices = new IndexBuffer(device, mesh.Indices.Length * sizeof(int), Usage.WriteOnly, Pool.Default, false);
        this.Indices.Lock(0, 0, LockFlags.None).WriteRange(mesh.Indices);
        this.Indices.Unlock();
    }


    public void Dispose() {
        DisposableRef.Dispose(ref this.Vertices);
        DisposableRef.Dispose(ref this.Indices);
    }
    
}