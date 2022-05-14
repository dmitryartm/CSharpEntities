using System;
using glTFViewer.Utils;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Components;


public struct MeshInstanceArrayGpu : IDisposable {
    public VertexBuffer? TransformMatrices;
    public int Count;
    public int Stride;


    public bool BufferIsEmpty => this.TransformMatrices == null;


    public void Dispose() {
        DisposableRef.Dispose(ref this.TransformMatrices);
    }
    
}