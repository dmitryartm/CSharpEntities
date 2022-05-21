using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CSharpEntities;
using CSharpEntities.Option;
using CSharpEntities.Systems;
using glTFLoader;
using glTFLoader.Schema;
using glTFViewer.World.Components;
using SharpDX;
using SharpDX.Mathematics.Interop;


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
        if (this._loadedFile != null && this._fileToLoad != this._loadedFile) {
            this.DoClear();
        }

        if (this._fileToLoad != null && this._fileToLoad != this._loadedFile) {
            try {
                this.DoLoadFile(this._fileToLoad);
            }
            catch (Exception ex) {
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


    private static (Gltf gltf, byte[] binaryBuffer) ReadGltf(string fileName) {
        var gltf = Interface.LoadModel(fileName);

        var buffer = gltf.Buffers != null && gltf.Buffers.Any() && TryLoadBinaryBuffer(fileName, out var bytes)
            ? bytes
            : Array.Empty<byte>();

        return (gltf, buffer);


        static bool TryLoadBinaryBuffer(string fileName, out byte[] bytes) {
            try {
                bytes = Interface.LoadBinaryBuffer(fileName);
                return true;
            }
            catch {
                // ignored
                bytes = Array.Empty<byte>();
                return false;
            }
        }
    }


    private void DoClear() {
        this.Entities.ForEach((ref MeshInstanceArrayGpu instances) => instances.Dispose()).Execute();
        this.Entities.ForEach((ref MeshDataGpuComponent mesh) => mesh.Dispose()).Execute();
        this.Entities.DestroyAllEntities(archetype => !archetype.HasComponents<MainCamera>());
        this._loadedFile = null;
    }


    private unsafe void DoLoadFile(string fileName) {
        var (gltf, gltfBinary) = ReadGltf(fileName);

        var meshEntityByIndex = new Dictionary<int, Entity>();

        CreateMeshEntities();
        CreateNodeEntities();
        UpdateCamera();
        CreateMeshInstanceArrays();

        this._loadedFile = fileName;


        void CreateMeshEntities() {
            var opaqueMeshArchetype = this.Entities.Archetype<
                MeshDataComponent,
                MeshDataGpuComponent,
                MeshId,
                MeshInstanceList,
                BoundingBox
            >();

            var transparentMeshArchetype = opaqueMeshArchetype.AddComponents<TransparentTag>();

            var bufferViews = gltf.BufferViews;
            var accessors = gltf.Accessors;

            var buffers = gltf.Buffers;
            if (buffers == null) {
                return;
            }

            var buffersCount = buffers.Length;
            var buffersData = new byte[buffersCount][];
            for (var i = 0; i < buffersCount; ++i) {
                buffersData[i] = gltf.LoadBinaryBuffer(i, _ => gltfBinary);
            }


            var gltfMaterials = gltf.Materials;
            var gltfMeshes = gltf.Meshes;

            var meshesCount = gltfMeshes.Length;

            for (var i = 0; i < meshesCount; ++i) {
                var gltfMesh = gltfMeshes[i];
                var primitive = gltfMesh.Primitives[0];

                var positionsAccessorIndex = primitive.Attributes["POSITION"];
                var normalsAccessorIndex = primitive.Attributes["NORMAL"];
                var indicesAccessorIndex = primitive.Indices!.Value;

                var positions = GetVector3Collection(positionsAccessorIndex, out var minPosition, out var maxPosition);
                var normals = GetVector3Collection(normalsAccessorIndex, out _, out _);
                var indices = GetIndicesCollection(indicesAccessorIndex);

                RawColorBGRA[]? colors = null;
                if (primitive.Attributes.TryGetValue("COLOR_0", out var colorsAccessorIndex)) {
                    colors = GetColorCollection(colorsAccessorIndex);
                }

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
                };

                for (var vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex) {
                    mesh.Vertices[vertexIndex] =
                        new VertexData(
                            positions[vertexIndex],
                            normals[vertexIndex],
                            colors?[vertexIndex] ?? meshColor);
                }

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
                this.Entities.Set(meshEntity, new BoundingBox(minPosition, maxPosition));
                this.Entities.Set(meshEntity, new MeshId { Index = i });
                this.Entities.Set(meshEntity,
                    new MeshInstanceList { TransformMatrices = new List<Matrix>() }
                );
                meshEntityByIndex[i] = meshEntity;
            }


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


        void CreateNodeEntities() {
            var boxCornersBuffer = new Vector3[8];

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

            var nodeWithoutMeshArchetype = this.Entities.Archetype<NodeTag, Transform>();
            var nodeWithMeshArchetype = nodeWithoutMeshArchetype.AddComponents<MeshEntity, BoundingBox>();
            var nodeWithTransparentMeshArchetype = nodeWithMeshArchetype.AddComponents<TransparentTag>();

            foreach (var node in rootNodes) {
                AddNodeRec(node, Option.None);
            }


            void AddNodeRec(int nodeIndex, Option<Matrix> parentMatrixOpt) {
                var node = gltf.Nodes[nodeIndex];

                var trs =
                    Matrix.Scaling(new Vector3(node.Scale))
                    * Matrix.RotationQuaternion(new Quaternion(node.Rotation))
                    * Matrix.Translation(new Vector3(node.Translation));

                var matrix = trs * new Matrix(node.Matrix);
                if (parentMatrixOpt.TryGetValue(out var parentMatrix)) {
                    matrix *= parentMatrix;
                }

                Entity entity;
                if (node.Mesh != null) {
                    var meshEntity = meshEntityByIndex[node.Mesh.Value];
                    var meshInstancesList = meshEntity.Get<MeshInstanceList>();
                    ref var meshBounds = ref meshEntity.Ref<BoundingBox>();

                    meshInstancesList.TransformMatrices.Add(matrix);

                    meshBounds.GetCorners(boxCornersBuffer);
                    for (var i = 0; i < boxCornersBuffer.Length; ++i) {
                        ref var point = ref boxCornersBuffer[i];
                        var point4 = new Vector4(point, 1f);
                        point = (Vector3) Vector4.Transform(point4, matrix);
                    }

                    entity = this.Entities.CreateEntity(
                        meshEntity.Has<TransparentTag>() ? nodeWithTransparentMeshArchetype : nodeWithMeshArchetype
                    );
                    entity.Set(new MeshEntity { Entity = meshEntity });
                    entity.Set(BoundingBox.FromPoints(boxCornersBuffer));
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
            var meshesWithoutInstances = new List<Entity>();
            var meshesWithInstanceArray = new List<Entity>();

            this.Entities.ForEach((in Entity entity, ref MeshInstanceList instances) => {
                var count = instances.TransformMatrices.Count;
                if (count == 0) {
                    meshesWithoutInstances.Add(entity);
                }
                else if (count > 1) {
                    meshesWithInstanceArray.Add(entity);
                }
            }).Execute();

            foreach (var entity in meshesWithoutInstances) {
                this.Entities.DestroyEntity(entity);
            }

            foreach (var entity in meshesWithInstanceArray) {
                if (!entity.Has<TransparentTag>()) {
                    this.Entities.AddComponents<MeshInstanceArrayGpu>(entity);
                }
            }
        }


        void UpdateCamera() {
            var sceneBoundsOpt = Option.NoneOf<BoundingBox>();
            this.Entities.ForEach(archetype => archetype.HasComponents<NodeTag>(), (ref BoundingBox box) => {
                if (sceneBoundsOpt.TryGetValue(out var sceneBounds)) {
                    BoundingBox.Merge(ref box, ref sceneBounds, out var newSceneBounds);
                    sceneBoundsOpt = Option.Some(newSceneBounds);
                }
                else {
                    sceneBoundsOpt = Option.Some(box);
                }
            }).Execute();

            ref var camera = ref this.Entities.Single<MainCamera>().Ref<MainCamera>();
            var sceneBox = sceneBoundsOpt.IfNone(new BoundingBox(Vector3.Zero, Vector3.One));
            var sceneSphere = BoundingSphere.FromBox(sceneBox);
            camera.Origin = sceneSphere.Center;
            var minimumSize = 2.01f * sceneSphere.Radius;
            camera.MinimumFrustumSize = new Size2F(minimumSize, minimumSize);
            camera.Phi = 1.5f * (float) Math.PI;
            camera.Radius = sceneSphere.Radius * 1.5f;
            camera.ZNear = 0f;
            camera.ZFar = sceneSphere.Radius * 3f;
        }
    }


    #endregion


}