using System;
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
        this.Add(DrawSubsystem());
        return;


        #region Functions


        ComponentSystem DrawSubsystem() {
            return new InlineSystem(this.World, "Draw") {
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
                        0,
                        this._screenQuad.TrianglesCount);
                }),

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
                            archetype => !archetype.HasComponents<TransparentTag>(),
                            (ref MeshDataGpuComponent mesh, ref MeshSingleInstance instance) => {
                                DrawSingle(this.Device, mesh, instance.TransformMatrix);
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

                new InlineSystem(this.World, "Transparent", _ => {
                    this.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                }) {
                    new InlineSystem(this.World, "Depth Pass") {
                        // new InlineQuerySystem(this.World, "Instanced",
                        //     this.Entities.ForEach(
                        //         archetype => archetype.HasComponents<TransparentTag>(),
                        //         (ref MeshDataGpuComponent mesh, ref MeshInstancesGpu instances) =>
                        //             DrawInstanced(this.Device, mesh, instances),
                        //         drawInstanced => {
                        //             if (drawInstanced.Query.HasEntities) {
                        //                 this.Device.SetVertexShader(this._shaders
                        //                     .VS_DiffuseOpaqueUniformColorInstanced);
                        //                 this.Device.SetPixelShader(this._shaders.PS_DepthOnly);
                        //                 drawInstanced.Execute();
                        //                 this.Device.SetStreamSourceFrequency(0, 1, StreamSource.IndexedData);
                        //             }
                        //         }),
                        new InlineQuerySystem(this.World, "Single",
                            this.Entities.ForEach(
                                archetype => archetype.HasComponents<TransparentTag>(),
                                (ref MeshDataGpuComponent mesh, ref MeshSingleInstance instance) => {
                                    DrawSingle(this.Device, mesh, instance.TransformMatrix);
                                }),
                            drawVisibleSingle => {
                                if (drawVisibleSingle.Query.HasEntities) {
                                    var shaders = this.Shaders;
                                    this.Device.SetVertexShader(shaders.VS_DiffuseOpaque);
                                    this.Device.SetPixelShader(shaders.PS_DepthOnly);
                                    drawVisibleSingle.Execute();
                                }
                            })
                    },

                    new InlineSystem(this.World, "Color Pass") {
                        // new InlineQuerySystem(this.World, "Instanced",
                        //     this.Entities.ForEach(
                        //         archetype => archetype.HasComponents<TransparentTag>(),
                        //         (ref MeshDataGpuComponent mesh, ref MeshInstancesGpu instances) =>
                        //             DrawInstanced(this.Device, mesh, instances),
                        //         drawInstanced => {
                        //             if (drawInstanced.Query.HasEntities) {
                        //                 this.Device.SetVertexShader(this._shaders
                        //                     .VS_DiffuseOpaqueVertexColorsInstanced);
                        //                 this.Device.SetPixelShader(this._shaders.PS_Lit);
                        //                 drawInstanced.Execute();
                        //                 this.Device.SetStreamSourceFrequency(0, 1, StreamSource.IndexedData);
                        //             }
                        //         }),
                        new InlineQuerySystem(this.World, "Single",
                            this.Entities.ForEach(
                                archetype => archetype.HasComponents<TransparentTag>(),
                                (ref MeshDataGpuComponent mesh, ref MeshSingleInstance instance) => {
                                    DrawSingle(this.Device, mesh, instance.TransformMatrix);
                                }),
                            drawVisibleSingle => {
                                if (drawVisibleSingle.Query.HasEntities) {
                                    var shaders = this.Shaders;
                                    this.Device.SetPixelShader(shaders.PS_Lit);
                                    this.Device.SetVertexShader(shaders.VS_DiffuseOpaqueVertexColors);
                                    drawVisibleSingle.Execute();
                                }
                            })
                    },
                },

                new InlineSystem(this.World, "D3DImage Update") {
                    new InlineSystem(this.World, "Image Lock", _ => this.Image.Lock()),
                    new InlineSystem(this.World, "Draw to Image Surface", _ => {
                        this.Device.SetRenderTarget(0, this.ImageSurface);
                        this.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1f, 0);
                        this.Device.StretchRectangle(this.RenderSurface, this.ImageSurface, TextureFilter.None);

                        // this.Device.StretchRectangle(this.RenderSurface, this.ImageSurface, TextureFilter.None);
                    }),

                    new InlineSystem(this.World, "End Scene", _ => this.Device.EndScene()),

                    new InlineSystem(this.World, "D3DImage Set Back Buffer", _ => {
                        this.Image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.ImageSurface.NativePointer);
                        this.Image.AddDirtyRect(new System.Windows.Int32Rect(0, 0, this.SurfaceSize.Width,
                            this.SurfaceSize.Height));
                    }),

                    new InlineSystem(this.World, "Image Unlock", _ => this.Image.Unlock())
                },
            };


            static void DrawInstanced(DeviceEx device, in MeshDataGpuComponent mesh,
                in MeshInstanceArrayGpu instances) {
                device.Indices = mesh.Indices;

                device.SetStreamSourceFrequency(0, instances.Count, StreamSource.IndexedData);
                device.SetStreamSource(0, mesh.Vertices, 0, sizeof(VertexData));
                device.SetStreamSource(1, instances.TransformMatrices, 0, instances.Stride);

                device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, mesh.VerticesCount, 0,
                    mesh.TrianglesCount);
            }


            static void DrawSingle(DeviceEx device, in MeshDataGpuComponent mesh, in Matrix localToWorld) {
                device.SetLocalToWorld(localToWorld);

                device.Indices = mesh.Indices;
                device.SetStreamSource(0, mesh.Vertices, 0, sizeof(VertexData));

                device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, mesh.VerticesCount, 0,
                    mesh.TrianglesCount);
            }
        }


        #endregion
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


    #endregion


}