using SharpDX;
using SharpDX.Mathematics.Interop;


namespace glTFViewer.World.Components;


public struct MeshInstance {
    public Matrix Matrix;
    public RawColorBGRA Color;


    public MeshInstance(in Matrix matrix, in RawColorBGRA color) {
        this.Matrix = matrix;
        this.Color = color;
    }
}