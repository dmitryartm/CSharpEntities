using CSlns.Entities;


namespace glTFViewer.World.Components;


public struct MeshEntity {
    public EntityId Entity;
    public Option<int> SelectedIndex;
    public Option<int> VisibleIndex;
}