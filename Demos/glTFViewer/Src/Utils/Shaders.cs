using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using SharpDX.Direct3D9;


namespace glTFViewer.Utils;


internal class Shaders : IDisposable {

    public Shaders(Device device) {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        this.shadersDir = Path.Combine(dir, "Resources", "Shaders", "Compiled");

        this.device = device;

        this.VS_DiffuseOpaque = this.LoadVertexShader("VS_DiffuseOpaque.o");
        this.VS_DiffuseOpaqueInstanced = this.LoadVertexShader("VS_DiffuseOpaqueInstanced.o");
        this.VS_DiffuseOpaqueUniformColorInstanced = this.LoadVertexShader("VS_DiffuseOpaqueUniformColorInstanced.o");
        this.VS_DiffuseOpaqueVertexColors = this.LoadVertexShader("VS_DiffuseOpaqueVertexColors.o");
        this.VS_DepthOnly = this.LoadVertexShader("VS_DepthOnly.o");
        this.VS_ScreenTexture = this.LoadVertexShader("VS_ScreenTexture.o");
        this.VS_DiffuseOpaqueVertexColorsInstanced = this.LoadVertexShader("VS_DiffuseOpaqueVertexColorsInstanced.o");
        this.VS_UnlitInstanced = this.LoadVertexShader("VS_UnlitInstanced.o");

        this.PS_Lit = this.LoadPixelShader("PS_Lit.o");
        this.PS_Unlit = this.LoadPixelShader("PS_Unlit.o");
        this.PS_DepthOnly = this.LoadPixelShader("PS_DepthOnly.o");
    }


    private readonly Device device;
    private readonly CompositeDisposable disposable = new CompositeDisposable();
    private readonly string shadersDir;


    private PixelShaderPtr LoadPixelShader(string name) {
        var ptr = this.device.LoadPixelShaderFromFile(Path.Combine(this.shadersDir, name));
        this.disposable.Add(ptr);
        return ptr;
    }


    private VertexShaderPtr LoadVertexShader(string name) {
        var ptr = this.device.LoadVertexShaderFromFile(Path.Combine(this.shadersDir, name));
        this.disposable.Add(ptr);
        return ptr;
    }


    public VertexShaderPtr VS_DiffuseOpaque { get; }
    public VertexShaderPtr VS_DiffuseOpaqueInstanced { get; }
    public VertexShaderPtr VS_DiffuseOpaqueUniformColorInstanced { get; }
    public VertexShaderPtr VS_DiffuseOpaqueVertexColors { get; }
    public VertexShaderPtr VS_DepthOnly { get; }
    public VertexShaderPtr VS_ScreenTexture { get; }
    public VertexShaderPtr VS_DiffuseOpaqueVertexColorsInstanced { get; }
    public VertexShaderPtr VS_UnlitInstanced { get; }

    public PixelShaderPtr PS_Lit { get; }
    public PixelShaderPtr PS_Unlit { get; }
    public PixelShaderPtr PS_DepthOnly { get; }


    public void Dispose() {
        this.disposable.Dispose();
    }

}