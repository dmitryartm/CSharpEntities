using SharpDX;
using SharpDX.Mathematics.Interop;


namespace glTFViewer.World.Components;


public struct VertexData {
    public Vector3 Position;
    public Vector3 Normal;
    public RawColorBGRA Color;


    public VertexData(Vector3 position, Vector3 normal, RawColorBGRA color) {
        this.Position = position;
        this.Normal = normal;
        this.Color = color;
    }
}