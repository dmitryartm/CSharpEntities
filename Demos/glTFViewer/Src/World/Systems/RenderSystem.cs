using System;
using System.Collections.Generic;
using System.Windows.Interop;
using CSlns.Entities;
using CSlns.Entities.Systems;
using glTFViewer.Utils;
using glTFViewer.World.Components;
using SharpDX;
using SharpDX.Direct3D9;


namespace glTFViewer.World.Systems;


public class RenderSystem : ComponentSystem<MainWorld> {

    public unsafe RenderSystem(MainWorld world) : base(world) {
        this.AddRange(
            new InlineSystem(this.World, "BeginScene", _ => {
                this.Device.BeginScene();

                var camera = this.Entities.Single<MainCamera>().Get<MainCamera>();

                var lightDir = camera.Origin - camera.EyePosition();
                lightDir.Normalize();

                this.Device.SetViewProj(camera.ViewProjMatrix());
                this.Device.SetLightDir(lightDir);
                this.Device.SetRenderState(RenderState.AlphaBlendEnable, false);

                this.Device.SetRenderTarget(0, this.RenderSurface);
                this.Device.SetRenderState(RenderState.ZEnable, true);
                this.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.LightGray, 1.0f, 0);

                var shaders = this.Shaders;
                this.Device.SetVertexShader(shaders.VS_ScreenTexture);
                this.Device.SetPixelShader(shaders.PS_Unlit);

                this.Device.Indices = this._screenQuad.Indices;
                this.Device.SetStreamSource(0, this._screenQuad.Vertices, 0, sizeof(VertexData));

                this.Device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, this._screenQuad.VerticesCount,
                    0, this._screenQuad.TrianglesCount);
            }),
            new InlineSystem(this.World, "Draw Meshes") {
                new InlineSystem(this.World, "Opaque") {
                    new InlineQuerySystem(this.World, "Instanced",
                        this.Entities.ForEach(
                            archetype => !archetype.HasComponents<TransparentTag>(),
                            (ref MeshDataGpuComponent mesh, ref MeshInstanceArrayGpu instances) =>
                                DrawInstanced(this.Device, mesh, instances)),
                        drawInstanced => {
                            if (drawInstanced.Query.HasEntities) {
                                var shaders = this.Shaders;
                                this.Device.SetVertexShader(shaders.VS_DiffuseOpaqueVertexColorsInstanced);
                                this.Device.SetPixelShader(shaders.PS_Lit);
                                drawInstanced.Execute();
                                this.Device.SetStreamSourceFrequency(0, 1, StreamSource.IndexedData);
                            }
                        }),

                    new InlineQuerySystem(this.World, "Single",
                        this.Entities.ForEach(
                            archetype =>
                                !archetype.HasComponents<TransparentTag>()
                                && !archetype.HasComponents<MeshInstanceArrayGpu>(),
                            (ref MeshDataGpuComponent mesh, ref MeshInstanceList instances) => {
                                foreach (var matrix in instances.TransformMatrices) {
                                    DrawSingle(this.Device, mesh, matrix);
                                }
                            }),
                        drawVisibleSingle => {
                            if (drawVisibleSingle.Query.HasEntities) {
                                var shaders = this.Shaders;
                                this.Device.SetPixelShader(shaders.PS_Lit);
                                this.Device.SetVertexShader(shaders.VS_DiffuseOpaqueVertexColors);
                                drawVisibleSingle.Execute();
                            }
                        }),
                },
                new DrawTransparentMeshesSystem(this),
            },
            new InlineSystem(this.World, "Update D3DImage and End Scene", _ => {
                this.Image.Lock();

                this.Device.SetRenderTarget(0, this.ImageSurface);
                this.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1f, 0);
                this.Device.StretchRectangle(this.RenderSurface, this.ImageSurface, TextureFilter.None);

                this.Device.EndScene();

                this.Image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.ImageSurface.NativePointer);
                this.Image.AddDirtyRect(new System.Windows.Int32Rect(0, 0, this.SurfaceSize.Width,
                    this.SurfaceSize.Height));

                this.Image.Unlock();
            })
        );
    }


    #region ComponentSystem


    protected override void OnStart() {
        this.DeviceManager = this.World.Get<DeviceManagerSystem>();
        this.SurfaceManager = this.World.Get<SurfaceManagerSystem>();

        HandleCreateAndDisposeDeviceRes();

        return;


        void HandleCreateAndDisposeDeviceRes() {
            this.DeviceManager.CreateDeviceRes.Subscribe(_ => {
                DisposableRef.Dispose(ref this._shaders);
                this._shaders = new Shaders(this.Device);

                var color0 = new Color(221, 221, 221, 255);
                var color1 = new Color(170, 170, 180, 255);

                var screenQuad = new MeshDataComponent {
                    Vertices = new VertexData[] {
                        new (new Vector3(-1f, -1f, 1f), default, color1),
                        new (new Vector3(1f, -1f, 1f), default, color1),
                        new (new Vector3(1f, 1f, 1f), default, color0),
                        new (new Vector3(-1f, 1f, 1f), default, color0),
                    },
                    Indices = new[] {
                        0, 1, 2,
                        2, 3, 0,
                    },
                };
                this._screenQuad.CreateBuffers(this.Device, screenQuad);
            }).DisposeWith(this.OnDispose);

            this.DeviceManager.DisposeDeviceRes.Subscribe(_ => {
                DisposableRef.Dispose(ref this._shaders);
                this._screenQuad.Dispose();
            }).DisposeWith(this.OnDispose);
        }
    }


    #endregion


    #region Private


    private Shaders? _shaders;


    private Shaders Shaders {
        get {
            if (this._shaders == null) {
                throw new NullReferenceException($"Shaders is null");
            }

            return this._shaders;
        }
    }


    private MeshDataGpuComponent _screenQuad;
    private SurfaceManagerSystem? SurfaceManager;
    private DeviceManagerSystem? DeviceManager;

    private Size2 SurfaceSize => this.SurfaceManager!.SurfaceSize;
    private D3DImage Image => this.World.D3DImage;
    private Surface RenderSurface => this.SurfaceManager!.RenderSurface!;
    private Surface ImageSurface => this.SurfaceManager!.ImageSurface!;
    private DeviceEx Device => this.DeviceManager!.Device!;


    private static unsafe void DrawSingle(DeviceEx device, in MeshDataGpuComponent mesh, in Matrix localToWorld) {
        device.SetLocalToWorld(localToWorld);
        device.Indices = mesh.Indices;
        device.SetStreamSource(0, mesh.Vertices, 0, sizeof(VertexData));
        device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, mesh.VerticesCount, 0,
            mesh.TrianglesCount);
    }


    private static unsafe void DrawInstanced(DeviceEx device, in MeshDataGpuComponent mesh,
        in MeshInstanceArrayGpu instances
    ) {
        device.Indices = mesh.Indices;
        device.SetStreamSourceFrequency(0, instances.Count, StreamSource.IndexedData);
        device.SetStreamSource(0, mesh.Vertices, 0, sizeof(VertexData));
        device.SetStreamSource(1, instances.TransformMatrices, 0, instances.Stride);
        device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, mesh.VerticesCount, 0,
            mesh.TrianglesCount);
    }


    #endregion


    private class DrawTransparentMeshesSystem : ComponentSystem {

        public DrawTransparentMeshesSystem(RenderSystem renderSystem) : base(renderSystem.World) {
            this._renderSystem = renderSystem;
            this._fillSortedTransparentMeshInstancesAction = this.Entities.ForEach(
                archetype => archetype.HasComponents<TransparentTag>(),
                (ref MeshEntity mesh, ref BoundingBox box, ref Transform transform) => {
                    var distanceSqr = (this._cameraPosition - box.Center).LengthSquared();
                    ref var meshDataGpu = ref this.Entities.Ref<MeshDataGpuComponent>(mesh.Entity);
                    this._sortedTransparentMeshesInstances.Add(distanceSqr, (meshDataGpu, transform.Matrix));
                });
        }


        protected override void OnStart() {
            this._renderSystem = this.World.Get<RenderSystem>();
        }


        protected override unsafe void OnExecute() {
            this._sortedTransparentMeshesInstances.Clear();
            this._cameraPosition = this.Entities.Single<MainCamera>().Ref<MainCamera>().EyePosition();
            this._fillSortedTransparentMeshInstancesAction.Execute();

            var device = this.Device;
            var shaders = this.Shaders;

            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetVertexShader(shaders.VS_DiffuseOpaqueVertexColors);
            for (var i = 0; i < this._sortedTransparentMeshesInstances.Count; ++i) {
                var (mesh, matrix) = this._sortedTransparentMeshesInstances.Values[i];

                device.SetLocalToWorld(matrix);
                device.Indices = mesh.Indices;
                device.SetStreamSource(0, mesh.Vertices, 0, sizeof(VertexData));

                // Depth pass
                device.SetPixelShader(shaders.PS_DepthOnly);
                device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, mesh.VerticesCount, 0,
                    mesh.TrianglesCount);

                // Color pass
                device.SetPixelShader(shaders.PS_Lit);
                device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, mesh.VerticesCount, 0,
                    mesh.TrianglesCount);
            }
        }


        private Vector3 _cameraPosition;

        private readonly SortedList<float, (MeshDataGpuComponent MeshData, Matrix Transform)>
            _sortedTransparentMeshesInstances = new (new DistanceComparer());

        private readonly QueryAction _fillSortedTransparentMeshInstancesAction;
        private RenderSystem _renderSystem;

        private Shaders Shaders => this._renderSystem.Shaders;
        private DeviceEx Device => this._renderSystem.Device;


        private class DistanceComparer : IComparer<float> {
            public int Compare(float x, float y) {
                return x < y ? 1 : -1;
            }
        }

    }


}