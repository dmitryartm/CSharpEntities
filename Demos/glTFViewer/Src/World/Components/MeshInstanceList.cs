using System.Collections.Generic;
using SharpDX;


namespace glTFViewer.World.Components;


/// <summary>
/// List of entities referencing mesh entity
/// </summary>
public struct MeshInstanceList {
    public List<Matrix> TransformMatrices;
}