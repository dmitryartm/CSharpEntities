using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CSlns.Entities;
using CSlns.Entities.Systems;
using glTFLoader;
using glTFLoader.Schema;
using glTFViewer.World.Components;
using SharpDX;
using SharpDX.Mathematics.Interop;
using MeshData = SharpDX.Direct3D9.MeshData;


namespace glTFViewer.World.Systems;


public class GltfLoaderSystem : ComponentSystem<MainWorld> {

    public GltfLoaderSystem(MainWorld world) : base(world) {
    }


    #region Public


    public void LoadFile(string fileName) {
        this._fileToLoad = fileName;
    }


    public void Unload() {
        this._fileToLoad = null;
    }


    #endregion


    #region ComponentSystem


    protected override void OnExecute() {
        var needToUnload = this._loadedFile != null && this._fileToLoad != this._loadedFile;
        if (needToUnload) {
            //TODO: unload
        }

        var needToLoadFile = this._fileToLoad != null && this._fileToLoad != this._loadedFile;
        if (needToLoadFile) {
            try {
                var gltfWithBuffer = GltfWithBuffer.ReadFromFile(this._fileToLoad!);
                var gltf = gltfWithBuffer.Gltf;

                var meshes = gltfWithBuffer.GetMeshDataComponents();
                var meshEntityByIndex = new Dictionary<int, Entity>();

                CreateMeshEntities();
                CreateNodeEntities();
                UpdateCamera();
                CreateMeshInstanceArrays();

                this._loadedFile = this._fileToLoad;


                void CreateMeshEntities() {
                    var opaqueMeshArchetype = this.Entities.Archetype<
                        MeshDataComponent,
                        MeshDataGpuComponent,
                        MeshId,
                        MeshInstanceList
                    >();

                    var transparentMeshArchetype = opaqueMeshArchetype.AddComponents<TransparentTag>();


                    for (var i = 0; i < meshes.Length; ++i) {
                        var mesh = meshes[i];

                        var isTransparent = false;
                        foreach (var vertex in mesh.Vertices) {
                            if (vertex.Color.A < 255) {
                                isTransparent = true;
                                break;
                            }
                        }

                        var meshEntity = this.Entities.CreateEntity(
                            isTransparent ? transparentMeshArchetype : opaqueMeshArchetype
                        );

                        this.Entities.Set(meshEntity, mesh);
                        this.Entities.Set(meshEntity, new MeshId { Index = i });
                        this.Entities.Set(meshEntity,
                            new MeshInstanceList { TransformMatrices = new List<Matrix>() });
                        meshEntityByIndex[i] = meshEntity;
                    }
                }


                void CreateNodeEntities() {
                    IEnumerable<int> rootNodes;
                    if (gltf.Scene != null) {
                        rootNodes = gltf.Scenes[gltf.Scene.Value].Nodes;
                    }
                    else if (gltf.Nodes.Any()) {
                        rootNodes = Enumerable.Range(0, gltf.Nodes.Length).Where(nodeIndex =>
                            !gltf.Nodes.Any(node => node.Children != null && node.Children.Contains(nodeIndex)));
                    }
                    else {
                        return;
                    }

                    var nodeWithoutMeshArchetype = this.Entities.Archetype<Transform>();
                    var nodeWithMeshArchetype = nodeWithoutMeshArchetype.AddComponents<MeshEntity>();

                    foreach (var node in rootNodes) {
                        AddNodeRec(node, Option.None);
                    }


                    void AddNodeRec(int nodeIndex, Option<Matrix> parentMatrixOpt) {
                        var node = gltf.Nodes[nodeIndex];

                        var matrix = new Matrix(node.Matrix);
                        if (parentMatrixOpt.TryGetValue(out var parentMatrix)) {
                            matrix *= parentMatrix;
                        }

                        Entity entity;
                        if (node.Mesh != null) {
                            var meshEntity = meshEntityByIndex[node.Mesh.Value];
                            entity = this.Entities.CreateEntity(nodeWithMeshArchetype);
                            entity.Set(new MeshEntity { Entity = meshEntity });
                            meshEntity.Get<MeshInstanceList>().TransformMatrices.Add(matrix);
                        }
                        else {
                            entity = this.Entities.CreateEntity(nodeWithoutMeshArchetype);
                        }

                        entity.Set(new Transform { Matrix = matrix });

                        if (node.Children is { Length: > 0 }) {
                            foreach (var childIndex in node.Children) {
                                AddNodeRec(childIndex, Option.Some(matrix));
                            }
                        }
                    }
                }


                void CreateMeshInstanceArrays() {
                    var entitiesWithInstanceArray = new List<Entity>();
                    var entitiesWithSingleInstance = new List<Entity>();

                    this.Entities.ForEach((in Entity entity, ref MeshInstanceList instances) => {
                        if (instances.TransformMatrices.Count == 1) {
                            entitiesWithSingleInstance.Add(entity);
                        }
                        else {
                            entitiesWithInstanceArray.Add(entity);
                        }
                    }).Execute();


                    foreach (var entity in entitiesWithInstanceArray) {
                        this.Entities.AddComponents<MeshInstanceArray, MeshInstanceArrayGpu>(entity);
                    }

                    foreach (var entity in entitiesWithSingleInstance) {
                        this.Entities.AddComponents<MeshSingleInstance>(entity);
                    }

                    this.Entities.ForEach((ref MeshInstanceList list, ref MeshInstanceArray array) => {
                        array.TransformMatrices = list.TransformMatrices.ToArray();
                    }).Execute();

                    this.Entities.ForEach((ref MeshInstanceList list, ref MeshSingleInstance instance) => {
                        instance.TransformMatrix = list.TransformMatrices.First();
                    }).Execute();

                    foreach (var entity in entitiesWithInstanceArray.Concat(entitiesWithSingleInstance)) {
                        this.Entities.RemoveComponent<MeshInstanceList>(entity);
                    }
                }


                void UpdateCamera() {
                    var sceneBoundsOpt = Option.NoneOf<BoundingBox>();
                    var cornersBuffer = new Vector3[8];
                    this.Entities.ForEach((ref MeshDataComponent mesh, ref MeshInstanceList instances) => {
                        if (mesh.MinPos.IsZero && mesh.MaxPos.IsZero) {
                            return;
                        }

                        var meshBounds = new BoundingBox(mesh.MinPos, mesh.MaxPos);

                        foreach (var matrix in instances.TransformMatrices) {
                            meshBounds.GetCorners(cornersBuffer);
                            for (var i = 0; i < cornersBuffer.Length; ++i) {
                                ref var point = ref cornersBuffer[i];
                                var point4 = new Vector4(point, 1f);
                                point = (Vector3) Vector4.Transform(point4, matrix);
                            }

                            var meshInstanceBounds = BoundingBox.FromPoints(cornersBuffer);
                            if (sceneBoundsOpt.TryGetValue(out var sceneBounds)) {
                                BoundingBox.Merge(ref meshInstanceBounds, ref sceneBounds, out var newSceneBounds);
                                sceneBoundsOpt = Option.Some(newSceneBounds);
                            }
                            else {
                                sceneBoundsOpt = Option.Some(meshInstanceBounds);
                            }
                        }
                    }).Execute();

                    ref var camera = ref this.Entities.Single<MainCamera>().Ref<MainCamera>();
                    var sceneBox = sceneBoundsOpt.IfNone(new BoundingBox(Vector3.Zero, Vector3.One));
                    var sceneSphere = BoundingSphere.FromBox(sceneBox);
                    camera.Origin = sceneSphere.Center;
                    var minimumSize = 2.01f * sceneSphere.Radius;
                    camera.MinimumFrustumSize = new Size2F(minimumSize, minimumSize);
                    camera.Radius = sceneSphere.Radius * 1.5f;
                    camera.ZNear = 0f;
                    camera.ZFar = sceneSphere.Radius * 3f;
                }
            }
            catch
                (Exception ex) {
                this.World.Logger.Error(ex, "Unable to load file {FileName}", this._fileToLoad);
                MessageBox.Show(
                    Application.Current.MainWindow!,
                    $"Unable to load file '{this._fileToLoad}':\r\n{ex.Message}",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                this._fileToLoad = null;
            }
        }
    }


    #endregion


    #region Private


    private string? _fileToLoad;
    private string? _loadedFile;


    #endregion


}


public record GltfWithBuffer(Gltf Gltf, byte[] BinaryBuffer) {


    public static GltfWithBuffer ReadFromFile(string fileName) {
        var gltf = Interface.LoadModel(fileName);

        var buffer =
            gltf.Buffers != null && gltf.Buffers.Any()
                ? Interface.LoadBinaryBuffer(fileName)
                : Array.Empty<byte>();

        return new GltfWithBuffer(gltf, buffer);
    }


    public unsafe MeshDataComponent[] GetMeshDataComponents() {
        var bufferViews = this.Gltf.BufferViews;
        var accessors = this.Gltf.Accessors;

        var buffers = this.Gltf.Buffers;
        if (buffers == null) {
            return Array.Empty<MeshDataComponent>();
        }

        var buffersCount = buffers.Length;
        var buffersData = new byte[buffersCount][];
        for (var i = 0; i < buffersCount; ++i) {
            buffersData[i] = this.Gltf.LoadBinaryBuffer(i, _ => this.BinaryBuffer);
        }


        var gltfMaterials = this.Gltf.Materials;
        var gltfMeshes = this.Gltf.Meshes;

        var meshesCount = gltfMeshes.Length;
        var meshes = new MeshDataComponent[meshesCount];

        for (var i = 0; i < meshesCount; ++i) {
            var gltfMesh = gltfMeshes[i];
            var primitive = gltfMesh.Primitives[0];

            var positionsAccessorIndex = primitive.Attributes["POSITION"];
            var normalsAccessorIndex = primitive.Attributes["NORMAL"];
            var indicesAccessorIndex = primitive.Indices!.Value;

            var positions = GetVector3Collection(positionsAccessorIndex, out var minPos, out var maxPos);
            var normals = GetVector3Collection(normalsAccessorIndex, out _, out _);
            var indices = GetIndicesCollection(indicesAccessorIndex);

            RawColorBGRA[]? colors = null;
            if (primitive.Attributes.TryGetValue("COLOR_0", out var colorsAccessorIndex)) {
                colors = GetColorCollection(colorsAccessorIndex);
            }

            var positionsAccessor = accessors[positionsAccessorIndex];
            var minPosition = new Vector3(positionsAccessor.Min);
            var maxPosition = new Vector3(positionsAccessor.Max);
            var boundingBox = new BoundingBox(minPosition, maxPosition);

            RawColorBGRA meshColor;
            if (primitive.Material.HasValue) {
                var materialIndex = primitive.Material.Value;
                var gltfMaterial = gltfMaterials[materialIndex];

                var meshColorArr = gltfMaterial.PbrMetallicRoughness.BaseColorFactor;
                meshColor = (Color) (new Color4 {
                    Red = meshColorArr[0],
                    Green = meshColorArr[1],
                    Blue = meshColorArr[2],
                    Alpha = meshColorArr[3],
                });
            }
            else {
                meshColor = new Color(144, 144, 144, 255);
            }

            var vertexCount = positions.Length;

            var mesh = new MeshDataComponent {
                Vertices = new VertexData[vertexCount],
                Indices = indices,
                MinPos = minPos,
                MaxPos = maxPos,
            };

            for (var vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex) {
                mesh.Vertices[vertexIndex] =
                    new VertexData(
                        positions[vertexIndex],
                        normals[vertexIndex],
                        colors?[vertexIndex] ?? meshColor);
            }

            meshes[i] = mesh;
        }

        return meshes;


        #region Functions


        Vector3[] GetVector3Collection(int accessorIndex, out Vector3 min, out Vector3 max) {
            var accessor = accessors[accessorIndex];
            var bufferViewIndex = accessor.BufferView!.Value;
            var bufferView = bufferViews[bufferViewIndex];
            var bufferData = buffersData[bufferView.Buffer];

            var count = accessor.Count;
            var values = new Vector3[count];
            var byteSize = count * 3 * sizeof(float);

            fixed (void* bufferDataPtr = bufferData, pointsPtr = values) {
                var dataPtr = (byte*) bufferDataPtr + bufferView.ByteOffset;
                System.Buffer.MemoryCopy(dataPtr, pointsPtr, byteSize, byteSize);
            }

            min = accessor.Min != null ? new Vector3(accessor.Min) : Vector3.Zero;
            max = accessor.Max != null ? new Vector3(accessor.Max) : Vector3.Zero;

            return values;
        }


        RawColorBGRA[] GetColorCollection(int accessorIndex) {
            var accessor = accessors[accessorIndex];
            var bufferViewIndex = accessor.BufferView!.Value;
            var bufferView = bufferViews[bufferViewIndex];
            var bufferData = buffersData[bufferView.Buffer];

            var count = accessor.Count;
            var values = new Color[count];
            var byteSize = count * sizeof(Color);

            fixed (void* bufferDataPtr = bufferData, colorsPtr = values) {
                var dataPtr = (byte*) bufferDataPtr + bufferView.ByteOffset;
                System.Buffer.MemoryCopy(dataPtr, colorsPtr, byteSize, byteSize);
            }

            var colorValues = new RawColorBGRA[count];
            for (var i = 0; i < count; ++i) {
                colorValues[i] = values[i];
            }

            return colorValues;
        }


        int[] GetIndicesCollection(int accessorIndex) {
            var accessor = accessors[accessorIndex];
            var bufferViewIndex = accessor.BufferView!.Value;
            var bufferView = bufferViews[bufferViewIndex];
            var bufferData = buffersData[bufferView.Buffer];

            var count = accessor.Count;
            var values = new int[count];

            if (accessor.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_INT) {
                var byteSize = count * sizeof(int);
                fixed (void* bufferDataPtr = bufferData, pointsPtr = values) {
                    var dataPtr = (byte*) bufferDataPtr + bufferView.ByteOffset;
                    System.Buffer.MemoryCopy(dataPtr, pointsPtr, byteSize, byteSize);
                }
            }
            else if (accessor.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_SHORT) {
                var shortValues = new ushort[count];
                var byteSize = count * sizeof(ushort);
                fixed (void* bufferDataPtr = bufferData, pointsPtr = shortValues) {
                    var dataPtr = (byte*) bufferDataPtr + bufferView.ByteOffset;
                    System.Buffer.MemoryCopy(dataPtr, pointsPtr, byteSize, byteSize);
                }

                for (var i = 0; i < count; ++i) {
                    values[i] = shortValues[i];
                }
            }
            else {
                throw new InvalidOperationException(
                    $"Cannot create index collection. Unsupported ComponentType {accessor.ComponentType} in GLTF file.");
            }

            return values;
        }


        #endregion
    }


}