using SharpDX;


namespace glTFViewer.World.Components;


public struct MeshDataComponent {
    public Vector3 MinPos;
    public Vector3 MaxPos;
    public VertexData[] Vertices;
    public int[] Indices;
}