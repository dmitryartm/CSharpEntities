using System;
using System.Reactive.Disposables;
using glTFViewer.Utils;
using SharpDX;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Components;


public struct MeshInstanceArray {
    public MeshInstance[] Visible;
    public int VisibleCount;

    public Matrix[] Outlined;
    public int OutlinedCount;
}


public struct MeshInstanceArrayGpu : IDisposable {
    public VertexBuffer? Visible;
    public VertexBuffer? Outlined;


    public void Dispose() {
        DisposableRef.Dispose(ref this.Visible);
        DisposableRef.Dispose(ref this.Outlined);
    }
}